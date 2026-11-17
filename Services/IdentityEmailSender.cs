using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Point_Pharma.Services
{
    public interface IEmailMessageTransport
    {
        Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default);
    }

    public sealed class IdentityEmailSender : IEmailSender<IdentityUser>
    {
        private readonly IEmailMessageTransport _transport;
        private readonly IConfiguration _configuration;

        public IdentityEmailSender(IEmailMessageTransport transport, IConfiguration configuration)
        {
            _transport = transport;
            _configuration = configuration;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_configuration["Email:SmtpUser"])
            && !string.IsNullOrWhiteSpace(_configuration["Email:SmtpAppPassword"]);

        public Task SendConfirmationLinkAsync(IdentityUser user, string email, string confirmationLink)
        {
            var content = $"Para confirmar o e-mail da sua conta PointPharma, acesse:\n\n{confirmationLink}";
            return EnviarAsync(email, "Confirme seu e-mail · PointPharma", content);
        }

        public Task SendPasswordResetCodeAsync(IdentityUser user, string email, string resetCode)
        {
            var content = string.Join(
                Environment.NewLine,
                "Recebemos uma solicitação para redefinir a senha da sua conta PointPharma.",
                "",
                "Digite este código na tela de recuperação do aplicativo:",
                resetCode,
                "",
                "O código expira em 30 minutos. Se você não solicitou a redefinição, ignore esta mensagem.");

            return EnviarAsync(email, "Código de recuperação · PointPharma", content);
        }

        public Task SendPasswordResetLinkAsync(IdentityUser user, string email, string resetLink)
        {
            var content = $"Para redefinir sua senha PointPharma, acesse:\n\n{resetLink}";
            return EnviarAsync(email, "Redefinição de senha · PointPharma", content);
        }

        private Task EnviarAsync(string destino, string assunto, string texto)
        {
            var remetente = _configuration["Email:SmtpUser"];
            if (!IsConfigured || string.IsNullOrWhiteSpace(remetente))
            {
                throw new InvalidOperationException("O envio de e-mail não está configurado.");
            }

            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress(
                _configuration["Email:FromName"] ?? "PointPharma",
                remetente));
            mensagem.To.Add(MailboxAddress.Parse(destino));
            mensagem.Subject = assunto;
            mensagem.Body = new TextPart("plain") { Text = texto };

            return _transport.SendAsync(mensagem);
        }
    }
}
