using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Point_Pharma.Services;
using MimeKit;

namespace PointPharma.IntegrationTests;

public sealed class IdentityEmailSenderTests
{
    [Fact]
    public async Task Password_reset_email_contains_the_code_and_uses_configured_sender()
    {
        var transport = new CapturingTransport();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Email:SmtpUser"] = "sender@example.test",
                ["Email:SmtpAppPassword"] = "test-app-password",
                ["Email:FromName"] = "PointPharma"
            })
            .Build();
        var sender = new IdentityEmailSender(transport, configuration);

        await sender.SendPasswordResetCodeAsync(
            new IdentityUser(),
            "cliente@example.test",
            "codigo-de-teste-123456");

        Assert.True(sender.IsConfigured);
        Assert.Equal("cliente@example.test", transport.Message!.To.Mailboxes.Single().Address);
        Assert.Equal("sender@example.test", transport.Message.From.Mailboxes.Single().Address);
        Assert.Equal("Código de recuperação · PointPharma", transport.Message.Subject);
        Assert.Contains("codigo-de-teste-123456", transport.Message.TextBody);
        Assert.Contains("30 minutos", transport.Message.TextBody);
    }

    private sealed class CapturingTransport : IEmailMessageTransport
    {
        public MimeMessage? Message { get; private set; }

        public Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
        {
            Message = message;
            return Task.CompletedTask;
        }
    }
}
