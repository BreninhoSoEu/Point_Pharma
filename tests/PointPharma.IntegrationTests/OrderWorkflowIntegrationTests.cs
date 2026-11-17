using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MimeKit;
using Point_Pharma;
using Point_Pharma.Data;
using Point_Pharma.Models;
using Point_Pharma.Services;

namespace PointPharma.IntegrationTests;

public sealed class OrderWorkflowIntegrationTests : IAsyncLifetime
{
    private readonly string _databaseName = $"PointPharma_Integration_{Guid.NewGuid():N}";
    private string _databaseConnectionString = string.Empty;
    private SqlServerApiFactory? _factory;
    private HttpClient? _client;

    [Fact]
    public async Task Account_guest_link_authenticated_purchase_concurrency_and_rollback_work()
    {
        await CreateTestDatabaseAsync();

        _factory = new SqlServerApiFactory(_databaseConnectionString);
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        int produtoPrincipalId;
        int produtoConcorrenteId;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(_databaseName, db.Database.GetDbConnection().Database);
            await db.Database.MigrateAsync();

            var farmacia = new Farmacias
            {
                Nome = "Farmácia de integração",
                Endereco = "Rua de Testes, 1",
                Contato = "(11) 0000-0000",
                HorarioFuncionamento = "08:00–20:00",
                Cnpj = "00.000.000/0001-00",
                Email = "farmacia@example.test"
            };
            db.Farmacias.Add(farmacia);
            await db.SaveChangesAsync();

            var produtoPrincipal = new Produto
            {
                Nome = "Produto de integração",
                Descricao = "Registro isolado para validação automatizada",
                Categoria = "Teste",
                Preco = 12.50m,
                QuantidadeEstoque = 5,
                FarmaciaId = farmacia.Id
            };
            var produtoConcorrente = new Produto
            {
                Nome = "Produto concorrente",
                Descricao = "Estoque unitário para requisições simultâneas",
                Categoria = "Teste",
                Preco = 7.25m,
                QuantidadeEstoque = 1,
                FarmaciaId = farmacia.Id
            };
            db.Produtos.AddRange(produtoPrincipal, produtoConcorrente);
            await db.SaveChangesAsync();
            produtoPrincipalId = produtoPrincipal.Id;
            produtoConcorrenteId = produtoConcorrente.Id;
        }

        var client = _client!;

        // A visitor order is created before the account exists.
        var pedidoVisitanteResponse = await CriarPedidoAsync(client, produtoPrincipalId);
        Assert.Equal(HttpStatusCode.Created, pedidoVisitanteResponse.StatusCode);
        var codigoVisitante = await LerCodigoPedidoAsync(pedidoVisitanteResponse);

        var email = $"integracao-{Guid.NewGuid():N}@example.test";
        var cadastroResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            nome = "Usuário de Integração",
            email,
            senha = "TesteSeguro123",
            dataNascimento = "1992-04-15"
        });
        Assert.Equal(HttpStatusCode.OK, cadastroResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync("/api/identity/login?useCookies=false", new
        {
            email,
            password = "TesteSeguro123"
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        using var loginJson = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var accessToken = loginJson.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var perfilResponse = await client.GetAsync("/api/auth/perfil");
        Assert.Equal(HttpStatusCode.OK, perfilResponse.StatusCode);
        using (var perfilJson = JsonDocument.Parse(await perfilResponse.Content.ReadAsStringAsync()))
        {
            Assert.Equal("Usuário de Integração", perfilJson.RootElement.GetProperty("nome").GetString());
            Assert.Equal(email, perfilJson.RootElement.GetProperty("email").GetString());
        }

        var atualizarPerfilResponse = await client.PutAsJsonAsync("/api/auth/perfil", new
        {
            nome = "Perfil Atualizado",
            dataNascimento = "1991-08-20"
        });
        Assert.Equal(HttpStatusCode.OK, atualizarPerfilResponse.StatusCode);
        using (var perfilAtualizado = JsonDocument.Parse(await atualizarPerfilResponse.Content.ReadAsStringAsync()))
        {
            Assert.Equal("Perfil Atualizado", perfilAtualizado.RootElement.GetProperty("nome").GetString());
            Assert.Equal("1991-08-20", perfilAtualizado.RootElement.GetProperty("dataNascimento").GetString());
        }

        var recuperacaoResponse = await client.PostAsJsonAsync("/api/auth/forgot-password", new { email });
        var recuperacaoEmailDesconhecido = await client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new { email = $"ausente-{Guid.NewGuid():N}@example.test" });
        Assert.Equal(HttpStatusCode.OK, recuperacaoResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, recuperacaoEmailDesconhecido.StatusCode);
        using (var recuperacaoJson = JsonDocument.Parse(await recuperacaoResponse.Content.ReadAsStringAsync()))
        {
            using var desconhecidoJson = JsonDocument.Parse(await recuperacaoEmailDesconhecido.Content.ReadAsStringAsync());
            Assert.Equal(
                recuperacaoJson.RootElement.GetProperty("message").GetString(),
                desconhecidoJson.RootElement.GetProperty("message").GetString());
        }
        Assert.Single(_factory.EmailTransport.Messages);
        var codigoRecuperacao = _factory.EmailTransport.LerCodigoRecuperacao();

        var redefinicaoResponse = await client.PostAsJsonAsync("/api/auth/reset-password", new
        {
            email,
            codigo = codigoRecuperacao,
            novaSenha = "SenhaNova123"
        });
        Assert.Equal(HttpStatusCode.OK, redefinicaoResponse.StatusCode);

        var novoLoginResponse = await client.PostAsJsonAsync("/api/identity/login?useCookies=false", new
        {
            email,
            password = "SenhaNova123"
        });
        Assert.Equal(HttpStatusCode.OK, novoLoginResponse.StatusCode);
        using (var novoLoginJson = JsonDocument.Parse(await novoLoginResponse.Content.ReadAsStringAsync()))
        {
            var novoToken = novoLoginJson.RootElement.GetProperty("accessToken").GetString();
            Assert.False(string.IsNullOrWhiteSpace(novoToken));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", novoToken);
        }

        var vincularResponse = await client.PostAsJsonAsync("/api/Pedidos/vincular-visitante", new
        {
            codigos = new[] { codigoVisitante }
        });
        Assert.Equal(HttpStatusCode.OK, vincularResponse.StatusCode);
        using (var vincularJson = JsonDocument.Parse(await vincularResponse.Content.ReadAsStringAsync()))
        {
            Assert.Equal(1, vincularJson.RootElement.GetProperty("pedidosVinculados").GetInt32());
        }

        var pedidoAutenticadoResponse = await CriarPedidoAsync(client, produtoPrincipalId);
        Assert.Equal(HttpStatusCode.Created, pedidoAutenticadoResponse.StatusCode);
        var codigoAutenticado = await LerCodigoPedidoAsync(pedidoAutenticadoResponse);

        var meusPedidosResponse = await client.GetAsync("/api/Pedidos/minhas");
        Assert.Equal(HttpStatusCode.OK, meusPedidosResponse.StatusCode);
        using (var meusPedidosJson = JsonDocument.Parse(await meusPedidosResponse.Content.ReadAsStringAsync()))
        {
            var codigos = meusPedidosJson.RootElement.EnumerateArray()
                .Select(pedido => pedido.GetProperty("codigoPedido").GetGuid())
                .ToHashSet();
            Assert.Contains(codigoVisitante, codigos);
            Assert.Contains(codigoAutenticado, codigos);
        }

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var usuarioId = await db.Users.Where(usuario => usuario.Email == email)
                .Select(usuario => usuario.Id)
                .SingleAsync();
            var pedidoVinculado = await db.Pedidos.SingleAsync(pedido => pedido.CodigoPedido == codigoVisitante);
            var pedidoAutenticado = await db.Pedidos.SingleAsync(pedido => pedido.CodigoPedido == codigoAutenticado);

            Assert.Equal(usuarioId, pedidoVinculado.UsuarioId);
            Assert.Equal(usuarioId, pedidoAutenticado.UsuarioId);
            Assert.Equal(3, await db.Produtos.Where(produto => produto.Id == produtoPrincipalId)
                .Select(produto => produto.QuantidadeEstoque)
                .SingleAsync());
        }

        // Eight buyers compete for the final unit. Exactly one order may succeed.
        var respostasConcorrentes = await Task.WhenAll(
            Enumerable.Range(0, 8)
                .Select(_ => CriarPedidoAsync(client, produtoConcorrenteId)));
        Assert.Single(respostasConcorrentes, resposta => resposta.StatusCode == HttpStatusCode.Created);
        Assert.Equal(7, respostasConcorrentes.Count(resposta => resposta.StatusCode == HttpStatusCode.Conflict));

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(0, await db.Produtos.Where(produto => produto.Id == produtoConcorrenteId)
                .Select(produto => produto.QuantidadeEstoque)
                .SingleAsync());
        }

        // Force a SaveChanges failure after stock has been decremented inside the request transaction.
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var maiorIdPedido = await db.Pedidos.MaxAsync(pedido => (int?)pedido.Id) ?? 0;
            // SQL Server requires a literal in a CHECK constraint; this value comes from the isolated test database's identity column.
#pragma warning disable EF1002
            await db.Database.ExecuteSqlRawAsync(
                $"ALTER TABLE dbo.Pedidos ADD CONSTRAINT CK_Test_RejectConfirmedPedido CHECK ([Id] <= {maiorIdPedido} OR [Status] <> N'Confirmado')");
#pragma warning restore EF1002
        }

        int pedidosAntesDaFalha;
        int estoqueAntesDaFalha;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            pedidosAntesDaFalha = await db.Pedidos.CountAsync();
            estoqueAntesDaFalha = await db.Produtos.Where(produto => produto.Id == produtoPrincipalId)
                .Select(produto => produto.QuantidadeEstoque)
                .SingleAsync();
        }

        HttpResponseMessage? respostaFalha = null;
        Exception? excecaoFalha = null;
        try
        {
            respostaFalha = await CriarPedidoAsync(client, produtoPrincipalId);
        }
        catch (Exception exception)
        {
            excecaoFalha = exception;
        }

        Assert.True(
            respostaFalha?.StatusCode == HttpStatusCode.InternalServerError || EhFalhaDeGravacaoSql(excecaoFalha),
            "A gravação deve falhar ao violar a constraint de teste.");

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(pedidosAntesDaFalha, await db.Pedidos.CountAsync());
            Assert.Equal(estoqueAntesDaFalha, await db.Produtos.Where(produto => produto.Id == produtoPrincipalId)
                .Select(produto => produto.QuantidadeEstoque)
                .SingleAsync());
        }
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_client is not null)
        {
            _client.Dispose();
        }

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await DropTestDatabaseAsync();
    }

    private async Task CreateTestDatabaseAsync()
    {
        var baseConnectionString = await ObterConexaoAdministrativaAsync();
        var masterConnection = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = "master"
        };

        await using var connection = new SqlConnection(masterConnection.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE [{_databaseName}]";
        await command.ExecuteNonQueryAsync();

        _databaseConnectionString = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = _databaseName
        }.ConnectionString;
    }

    private async Task DropTestDatabaseAsync()
    {
        if (string.IsNullOrWhiteSpace(_databaseConnectionString))
        {
            return;
        }

        var masterConnection = new SqlConnectionStringBuilder(_databaseConnectionString)
        {
            InitialCatalog = "master"
        };
        await using var connection = new SqlConnection(masterConnection.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_databaseName}]";
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<string> ObterConexaoAdministrativaAsync()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("POINTPHARMA_TEST_SQLSERVER");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment;
        }

        var projectRoot = new DirectoryInfo(AppContext.BaseDirectory);
        while (projectRoot is not null && !File.Exists(Path.Combine(projectRoot.FullName, "Point_Pharma.csproj")))
        {
            projectRoot = projectRoot.Parent;
        }

        if (projectRoot is null)
        {
            throw new InvalidOperationException("Não foi possível localizar Point_Pharma.csproj para configurar o banco de teste.");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(projectRoot.FullName)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var applicationConnection = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Defina POINTPHARMA_TEST_SQLSERVER com uma conexão administrativa ao SQL Server de teste.");
        return new SqlConnectionStringBuilder(applicationConnection).ConnectionString;
    }

    private static Task<HttpResponseMessage> CriarPedidoAsync(HttpClient client, int produtoId)
    {
        return client.PostAsJsonAsync("/api/Pedidos", new
        {
            formaPagamento = "pix",
            itens = new[] { new { produtoId, quantidade = 1 } }
        });
    }

    private static async Task<Guid> LerCodigoPedidoAsync(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("codigoPedido").GetGuid();
    }

    private static bool EhFalhaDeGravacaoSql(Exception? exception)
    {
        while (exception is not null)
        {
            if (exception is DbUpdateException or SqlException)
            {
                return true;
            }

            exception = exception.InnerException;
        }

        return false;
    }

    private sealed class CapturingEmailMessageTransport : IEmailMessageTransport
    {
        public List<MimeMessage> Messages { get; } = [];

        public Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public string LerCodigoRecuperacao()
        {
            var corpo = Messages.Single().TextBody
                ?? throw new InvalidOperationException("O e-mail de teste não contém texto.");
            var linhas = corpo.Split(
                ["\r\n", "\n"],
                StringSplitOptions.RemoveEmptyEntries);
            return Assert.Single(linhas, linha =>
                linha.Length >= 16
                && linha.All(caractere => char.IsAsciiLetterOrDigit(caractere) || caractere is '-' or '_'));
        }
    }

    private sealed class SqlServerApiFactory(string connectionString) : WebApplicationFactory<Program>
    {
        public CapturingEmailMessageTransport EmailTransport { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ApplicationDbContext>();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

                var emailConfiguration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Email:SmtpUser"] = "pointpharma@example.test",
                        ["Email:SmtpAppPassword"] = "integration-test-only",
                        ["Email:FromName"] = "PointPharma Teste"
                    })
                    .Build();
                services.RemoveAll<IdentityEmailSender>();
                services.AddSingleton(new IdentityEmailSender(EmailTransport, emailConfiguration));
                services.RemoveAll<IEmailMessageTransport>();
                services.AddSingleton<IEmailMessageTransport>(EmailTransport);
                services.RemoveAll<IEmailSender<IdentityUser>>();
                services.AddSingleton<IEmailSender<IdentityUser>>(serviceProvider =>
                    serviceProvider.GetRequiredService<IdentityEmailSender>());
            });
        }
    }
}
