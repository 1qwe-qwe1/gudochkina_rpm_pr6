using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gudochkina_pr3.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(string smtpServer, int smtpPort, string smtpUsername,
                           string smtpPassword, string fromEmail, string fromName = "Система уведомлений")
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUsername = smtpUsername;
            _smtpPassword = smtpPassword;
            _fromEmail = fromEmail;
            _fromName = fromName;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Timeout = 30000; 

                    System.Net.ServicePointManager.SecurityProtocol =
                        SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_fromEmail, _fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                        Priority = MailPriority.Normal
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (SmtpException smtpEx)
            {
                MessageBox.Show($"SMTP ошибка: {smtpEx.Message}\nСтатус код: {smtpEx.StatusCode}",
                    "Ошибка SMTP", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки email: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(1000, 9999).ToString();
        }

        public static string CreateVerificationEmailBody(string code, string username)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #FFCD3234; color: white; padding: 10px; text-align: center; }}
                    .code {{ font-size: 24px; font-weight: bold; color: #FFCD3234; padding: 20px; text-align: center; }}
                    .footer {{ margin-top: 20px; font-size: 12px; color: gray; text-align: center; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Код подтверждения</h2>
                    </div>
                    <p>Здравствуйте, {username}!</p>
                    <p>Для завершения операции используйте следующий код подтверждения:</p>
                    <div class='code'>{code}</div>
                    <p>Код действителен в течение 10 минут.</p>
                    <p>Если вы не запрашивали этот код, просто проигнорируйте данное письмо.</p>
                    <div class='footer'>
                        <p>Это автоматическое сообщение, пожалуйста, не отвечайте на него.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string CreatePasswordResetEmailBody(string code)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #FFCD3234; color: white; padding: 10px; text-align: center; }}
                    .code {{ font-size: 24px; font-weight: bold; color: #FFCD3234; padding: 20px; text-align: center; }}
                    .footer {{ margin-top: 20px; font-size: 12px; color: gray; text-align: center; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Восстановление пароля</h2>
                    </div>
                    <p>Здравствуйте!</p>
                    <p>Вы запросили восстановление пароля. Используйте следующий код для подтверждения:</p>
                    <div class='code'>{code}</div>
                    <p>Введите этот код в приложении, чтобы создать новый пароль.</p>
                    <p>Если вы не запрашивали восстановление пароля, просто проигнорируйте данное письмо.</p>
                    <div class='footer'>
                        <p>Это автоматическое сообщение, пожалуйста, не отвечайте на него.</p>
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}
