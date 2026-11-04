PointPharma --- Backend ASP.NET Core MVC
Backend do PointPharma, responsável por disponibilizar os dados de
farmácias e produtos para o aplicativo mobile e persistir as informações
no SQL Server.
Tecnologias
C#
ASP.NET Core MVC
Entity Framework Core
ASP.NET Identity
SQL Server
Swagger / OpenAPI
Funcionalidades
Cadastro, consulta, edição e exclusão de farmácias.
Cadastro, consulta, edição e exclusão de produtos.
Associação de produtos às farmácias por chave estrangeira.
Consulta dos produtos de uma farmácia específica.
Persistência com Entity Framework Core.
Autenticação e gerenciamento de usuários com ASP.NET Identity.
Estrutura principal
``` text
Point_Pharma/
├── Controllers/
│   ├── FarmaciasController.cs
│   └── ProdutosController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── Farmacias.cs
│   └── Produto.cs
├── Migrations/
├── Views/
├── wwwroot/
├── appsettings.json
└── Program.cs
```
A estrutura pode conter outros arquivos, páginas, modelos e componentes.
Requisitos
.NET SDK compatível com o projeto.
SQL Server instalado e em execução.
Banco de dados `Point_Pharma`.
Entity Framework Core Tools.
Visual Studio ou Visual Studio Code.
Banco de dados
Configure a connection string na seção `ConnectionStrings` do
`appsettings.json`. Exemplo de formato:
``` json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SEU_SERVIDOR;Database=Point_Pharma;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```
Substitua `SEU_SERVIDOR` pelo nome da instância SQL Server do ambiente.
Não publique senhas ou credenciais reais no repositório.
Instalação e execução
Na pasta do projeto, execute:
``` bash
dotnet restore
dotnet build
```
Se houver migrations pendentes, aplique-as:
``` bash
dotnet ef database update
```
Inicie o backend:
``` bash
dotnet run --urls "http://0.0.0.0:5000"
```
Swagger: `http://localhost:5000/swagger`
Endpoints da API
---
Método                  Rota                                    Descrição
---
GET                     `/api/Farmacias`                        Lista todas as
farmácias
GET                     `/api/Farmacias/{id}`                   Busca uma farmácia pelo
ID
POST                    `/api/Farmacias`                        Cadastra uma farmácia
PUT                     `/api/Farmacias/{id}`                   Atualiza uma farmácia
DELETE                  `/api/Farmacias/{id}`                   Exclui uma farmácia
GET                     `/api/Produtos`                         Lista todos os produtos
GET                     `/api/Produtos/Farmacia/{farmaciaId}`   Lista produtos de uma
farmácia
GET                     `/api/Produtos/{id}`                    Busca um produto pelo
ID
POST                    `/api/Produtos`                         Cadastra um produto
PUT                     `/api/Produtos/{id}`                    Atualiza um produto
DELETE                  `/api/Produtos/{id}`                    Exclui um produto
Relacionamento entre farmácias e produtos
Cada produto possui a propriedade `FarmaciaId`, que identifica a
farmácia à qual pertence. Uma farmácia pode possuir vários produtos, e
cada produto fica associado a uma farmácia.
Integração com o aplicativo mobile
O aplicativo Expo consome os endpoints deste backend por requisições
HTTP. Para testes em um celular físico, configure no aplicativo o IPv4
do computador que executa o backend e mantenha os dois dispositivos na
mesma rede Wi-Fi.
Observações
Confira a connection string ao executar em outro computador.
Aplique as migrations para manter o banco atualizado.
Utilize o Swagger para testar os endpoints.
Não armazene credenciais privadas no README ou em arquivos
versionados.
Projeto
PointPharma --- Backend responsável pelos dados de farmácias e
produtos.
