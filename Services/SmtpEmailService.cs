using GameStudioSite.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GameStudioSite.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendContactFormEmailAsync(ContactFormModel form)
        {
            var smtpSection = _config.GetSection("Smtp");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpSection["FromName"], smtpSection["FromAddress"]));
            message.To.Add(MailboxAddress.Parse(smtpSection["ToAddress"]));
            message.ReplyTo.Add(new MailboxAddress(form.FullName, form.Email));
            message.Subject = $"[Form Entry] - {form.Subject}";


            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;"">
                <h2 style=""color: #00004d;"">New Contact Form Submission</h2>
                <table style=""width: 100%; border-collapse: collapse; margin-bottom: 20px;"">
                    <tr>
                        <td style=""padding: 8px 0; font-weight: bold; width: 120px;"">Name:</td>
                        <td style=""padding: 8px 0;"">{System.Net.WebUtility.HtmlEncode(form.FullName)}</td>
                    </tr>
                    <tr>
                        <td style=""padding: 8px 0; font-weight: bold;"">Email:</td>
                        <td style=""padding: 8px 0;""><a href=""mailto:{form.Email}"">{System.Net.WebUtility.HtmlEncode(form.Email)}</a></td>
                    </tr>
                    <tr>
                        <td style=""padding: 8px 0; font-weight: bold;"">Subject:</td>
                        <td style=""padding: 8px 0;"">{System.Net.WebUtility.HtmlEncode(form.Subject)}</td>
                    </tr>
                </table>
                <div style=""background: #f4f4f8; padding: 16px; border-radius: 8px; white-space: pre-wrap;"">
                    {System.Net.WebUtility.HtmlEncode(form.Message)}
                </div>
            </div>",

                TextBody = $"New Contact Form Submission\n\n" +
                            $"Name: {form.FullName}\n" +
                            $"Email: {form.Email}\n" +
                            $"Subject: {form.Subject}\n\n" +
                            $"\nMessage:\n{form.Message}"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpSection["Host"], smtpSection.GetValue<int>("Port"), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpSection["Username"], smtpSection["Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}