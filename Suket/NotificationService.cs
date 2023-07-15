namespace Suket
{
    public interface INotificationService
    {
        Task SendEmailNotification(string email, string subject, string message);
    }
    public class NotificationService : INotificationService
    {
        private readonly ISuketEmailSender _emailSender;

        public NotificationService(ISuketEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task SendEmailNotification(string email, string subject, string message)
        {
            await _emailSender.SendEmailAsync(email, subject, message);
        }
    }
}
