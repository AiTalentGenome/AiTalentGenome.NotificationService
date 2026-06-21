using Microsoft.Extensions.Logging;

namespace AiTalentGenome.NotificationService.Infrastructure.Services;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}

public class EmailSender(ILogger<EmailSender> logger) : IEmailSender
{
    private readonly ILogger<EmailSender> _logger = logger;

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        // Имитируем сетевую задержку отправки почты
        await Task.Delay(100, ct);
        
        // В продакшене здесь будет реальный код MailKit / SMTP-клиента
        _logger.LogInformation("[Email Sent] To: {To} | Subject: {Subject}", to, subject);
    }
}