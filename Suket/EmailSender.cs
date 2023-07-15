using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Suket
{
    public interface ISuketEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class EmailSender : ISuketEmailSender
    {
        private readonly string _apiKey;

        public EmailSender(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SendGridClient(_apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("ehara@roadmint.co.jp", "Suketテストセンター"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message.Replace(Environment.NewLine, "<br>") // HTML形式のメッセージには、改行を <br> に置き換える
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            msg.SetClickTracking(false, false);

            await client.SendEmailAsync(msg);
        }
    }
}
