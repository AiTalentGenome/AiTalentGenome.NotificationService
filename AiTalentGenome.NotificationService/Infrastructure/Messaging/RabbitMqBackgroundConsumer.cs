using System.Text;
using System.Text.Json;
using AiTalentGenome.NotificationService.Application.EventHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AiTalentGenome.NotificationService.Infrastructure.Messaging;

public class RabbitMqBackgroundConsumer(IServiceProvider serviceProvider, ILogger<RabbitMqBackgroundConsumer> logger)
    : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel; // ФИКС: Используем IChannel вместо IModel
    
    private const string ExchangeName = "aitalentgenome.integration.exchange";
    private const string QueueName = "notification.service.queue";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // ФИКС: Инициализация теперь тоже асинхронная
        var factory = new ConnectionFactory() { HostName = "localhost" };
        
        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // ФИКС: Все декларации теперь *Async
        await _channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
        await _channel.QueueDeclareAsync(queue: QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(queue: QueueName, exchange: ExchangeName, routingKey: "vacancy.candidate.analyzed", cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(queue: QueueName, exchange: ExchangeName, routingKey: "identity.user.registered", cancellationToken: stoppingToken);

        // ФИКС: Для v7+ используется AsyncEventingBasicConsumer вместо EventingBasicConsumer
        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;

            logger.LogInformation("[RabbitMQ] Получено сообщение с ключом: {RoutingKey}", routingKey);

            try
            {
                using var scope = serviceProvider.CreateScope();
                
                if (routingKey == "vacancy.candidate.analyzed")
                {
                    var @event = JsonSerializer.Deserialize<CandidateAnalyzedIntegrationEvent>(message);
                    if (@event != null)
                    {
                        var handler = scope.ServiceProvider.GetRequiredService<CandidateAnalyzedEventHandler>();
                        await handler.HandleAsync(@event, stoppingToken);
                    }
                }

                // ФИКС: Асинхронное подтверждение
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка обработки сообщения из RabbitMQ. Оставляем в очереди.");
                // ФИКС: Асинхронный Nack
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        // ФИКС: Асинхронное чтение
        await _channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        // Держим воркер активным, пока не отменят токен
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        // В v7+ явное закрытие через Close() перенесено во внутренние механизмы жизненного цикла Async/Dispose
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}