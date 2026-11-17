using MailKit.Net.Smtp;
using MailKit.Security;
using MailKit;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Point_Pharma.Services
{
    public sealed class GmailSmtpEmailTransport : IEmailMessageTransport
    {
        private readonly IConfiguration _configuration;

        public GmailSmtpEmailTransport(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
        {
            var usuario = _configuration["Email:SmtpUser"];
            var senhaAplicativo = _configuration["Email:SmtpAppPassword"];
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senhaAplicativo))
            {
                throw new InvalidOperationException("O envio de e-mail não está configurado.");
            }

            using var cliente = new SmtpClient
            {
                Timeout = 15000
            };

            await cliente.ConnectAsync(
                "smtp.gmail.com",
                587,
                SecureSocketOptions.StartTls,
                cancellationToken);
            await cliente.AuthenticateAsync(usuario, senhaAplicativo, cancellationToken);
            await IMailTransportExtensions.SendAsync(cliente, message, cancellationToken);
            await cliente.DisconnectAsync(true, cancellationToken);
        }
    }
}
