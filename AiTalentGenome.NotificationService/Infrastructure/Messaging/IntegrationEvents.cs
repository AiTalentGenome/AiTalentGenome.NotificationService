namespace AiTalentGenome.NotificationService.Infrastructure.Messaging;

public record CandidateAnalyzedIntegrationEvent(
    Guid ApplicationId,
    Guid VacancyId,
    string CandidateName,
    string CandidateEmail,
    double AiScore,
    long OwnerId 
);

public record UserRegisteredIntegrationEvent(
    long UserId,
    string Email,
    string FirstName,
    string LastName,
    string Provider
);