using System.Net;
using System.Net.Mail;

namespace ai_dev_asst_api.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ITemplateService _templateService;

    public EmailService(IConfiguration config, ITemplateService templateService)
    {
        _config = config;
        _templateService = templateService;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string templateName, Dictionary<string, string> placeholders)
    {
        var htmlBody = await _templateService.RenderTemplateAsync(templateName, placeholders);

        var smtpHost = _config["Smtp:Host"]!;
        var smtpPort = int.Parse(_config["Smtp:Port"]!);
        var smtpUser = _config["Smtp:Username"]!;
        var smtpPass = _config["Smtp:Password"]!;
        var fromEmail = _config["Smtp:FromEmail"]!;
        var fromName = _config["Smtp:FromName"]!;

        using var message = new MailMessage();
        message.From = new MailAddress(fromEmail, fromName);
        message.To.Add(new MailAddress(toEmail));
        message.Subject = subject;
        message.Body = htmlBody;
        message.IsBodyHtml = true;

        using var client = new SmtpClient(smtpHost, smtpPort);
        client.Credentials = new NetworkCredential(smtpUser, smtpPass);
        client.EnableSsl = true;

        await client.SendMailAsync(message);
    }
}
