using AiTalentGenome.Contracts.Identity;
using AiTalentGenome.NotificationService.Application.EventHandlers;
using AiTalentGenome.NotificationService.Infrastructure.Messaging;
using AiTalentGenome.NotificationService.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ITemplateEngine, TemplateEngine>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();

builder.Services.AddGrpcClient<IdentityService.IdentityServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5001"); 
});

builder.Services.AddScoped<CandidateAnalyzedEventHandler>();

builder.Services.AddHostedService<RabbitMqBackgroundConsumer>();

var host = builder.Build();
await host.RunAsync();