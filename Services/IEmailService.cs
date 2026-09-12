namespace ai_dev_asst_api.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string templateName, Dictionary<string, string> placeholders);
}
