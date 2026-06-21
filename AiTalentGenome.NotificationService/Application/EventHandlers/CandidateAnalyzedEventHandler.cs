using AiTalentGenome.Contracts.Identity; // Твой gRPC контракт из первого запроса
using AiTalentGenome.NotificationService.Infrastructure.Messaging;
using AiTalentGenome.NotificationService.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace AiTalentGenome.NotificationService.Application.EventHandlers;

public class CandidateAnalyzedEventHandler(
    IEmailSender emailSender,
    ITemplateEngine templateEngine,
    IdentityService.IdentityServiceClient identityClient,
    ILogger<CandidateAnalyzedEventHandler> logger)
{
    public async Task HandleAsync(CandidateAnalyzedIntegrationEvent @event, CancellationToken ct)
    {
        try
        {
            // 1. Идем в IdentityService через gRPC, чтобы узнать Email и имя HR-менеджера по его OwnerId
            // В реальной жизни мы бы передавали AccessToken в gRPC, либо сделали внутренний эндпоинт для микросервисов (ApiKey/mTLS)
            var hrInfo = await identityClient.GetUserInfoAsync(new GetUserInfoRequest { AccessToken = "internal_system_token" }, cancellationToken: ct);
            
            if (!hrInfo.IsActive) return;

            // 2. Генерируем красивое HTML письмо
            string htmlBody = templateEngine.RenderAiAnalysisResultEmail(
                hrInfo.FirstName, 
                @event.CandidateName, 
                @event.AiScore, 
                @event.ApplicationId);

            // 3. Отправляем
            await emailSender.SendEmailAsync(
                hrInfo.Email, 
                $"AI Скоринг: Отклик {@event.CandidateName} проанализирован", 
                htmlBody, 
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке уведомления об анализе кандидата {AppId}", @event.ApplicationId);
            throw; // Пробрасываем выше, чтобы RabbitMQ сделал NACK и сообщение не потерялось
        }
    }
}