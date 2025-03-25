using MimeKit;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using MailKit.Net.Smtp;

namespace Login_Y_Registro.Service
{
    public class EmailService
    {

        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }


        public async Task<bool> EnviarCorreo(string destinatario, string asunto, string mensaje)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Mi Aplicación", _config["EmailSettings:SenderEmail"]));
                email.To.Add(MailboxAddress.Parse(destinatario));
                email.Subject = asunto;
                email.Body = new TextPart("html") { Text = mensaje };

                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                await smtp.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:SmtpPort"]), false);
                await smtp.AuthenticateAsync(_config["EmailSettings:SenderEmail"], _config["EmailSettings:SenderPassword"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando correo: {ex.Message}");
                return false;
            }
        }
    }
}
