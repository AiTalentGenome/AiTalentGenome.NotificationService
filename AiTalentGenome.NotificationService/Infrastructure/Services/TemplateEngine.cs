namespace AiTalentGenome.NotificationService.Infrastructure.Services;

public interface ITemplateEngine
{
    string RenderWelcomeEmail(string name, string provider);
    string RenderAiAnalysisResultEmail(string managerName, string candidateName, double score, Guid appId);
}

public class TemplateEngine : ITemplateEngine
{
    public string RenderWelcomeEmail(string name, string provider) =>
        $"<h1>Добро пожаловать в AiTalentGenome, {name}!</h1>" +
        $"<p>Вы успешно зарегистрировались через {provider}. Теперь вам доступен умный скоринг кандидатов.</p>";

    public string RenderAiAnalysisResultEmail(string managerName, string candidateName, double score, Guid appId) =>
        $"<p>Здравствуйте, {managerName}.</p>" +
        $"<h3>ИИ завершил анализ кандидата <b>{candidateName}</b>.</h3>" +
        $"<p>Оценка соответствия вакансии: <b>{score * 100:F0}%</b></p>" +
        $"<p><a href='https://aitalentgenome.ru/vacancies/applications/{appId}'>Перейти к просмотру аналитики в ЛК</a></p>";
}