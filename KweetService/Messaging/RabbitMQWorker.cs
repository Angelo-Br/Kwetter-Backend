using KweetService.DbContexts;
using KweetService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQLibrary;
using System.Text;

namespace KweetService.Messaging
{
    public class RabbitMQWorker : BackgroundService
    {
        private readonly ILogger<RabbitMQWorker> _logger;
        private ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;
        private readonly KweetServiceDatabaseContext _dbContext;

        public RabbitMQWorker(ILogger<RabbitMQWorker> logger, KweetServiceDatabaseContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            //string? password = Environment.GetEnvironmentVariable("redactieportaal_rabbitmq_pass") ?? throw new ArgumentNullException("No Password Found");

            _factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                DispatchConsumersAsync = true
            };

            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "user-service-exchange",
                                     durable: true,
                                     type: ExchangeType.Topic);

            _channel.QueueDeclare(queue: "user-service-queue",
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            var routingKeys = new[]
            {
                RoutingKeyType.UsernameUpdated,
                RoutingKeyType.UserDeleted,
            };
            foreach (var routingKey in routingKeys)
            {
                _channel.QueueBind(queue: "user-service-queue",
                                   exchange: "user-service-exchange",
                                   routingKey: routingKey);
            }

            _channel.BasicQos(prefetchSize: 0,
                              prefetchCount: 1,
                              global: false);

            _logger.LogInformation("RabbitMQWorker started");

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation($"Received message: {message} with routingkey: {eventArgs.RoutingKey}");

                switch (eventArgs.RoutingKey)
                {
                    case RoutingKeyType.UserCreated:
                        var createdUser = JsonConvert.DeserializeObject<User>(message);
                        if (createdUser == null)
                        {
                            _logger.LogCritical("Failed to deserialize object" + message);
                        }
                        _dbContext.Users.Add(createdUser);
                        _dbContext.SaveChanges();
                        break;
                    case RoutingKeyType.UsernameUpdated:
                        var newUser = JsonConvert.DeserializeObject<User>(message);
                        if (newUser == null)
                        {
                            _logger.LogCritical("Failed to deserialize object" + message);
                        }
                        var oldUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == newUser.Id);
                        if (oldUser == default)
                        {
                            _logger.LogCritical("User "+ newUser.Username +" doesnt exist " + "Error occured:"+ DateTime.Now);
                        }
                        else
                        {
                            oldUser.Username = newUser.Username;
                            await _dbContext.SaveChangesAsync();
                            _logger.LogInformation(newUser.Username+"User has his username changed");
                        }
                        break;
                    case RoutingKeyType.UserDeleted:
                        var deletedUser = JsonConvert.DeserializeObject<User>(message);
                        if (deletedUser == null)
                        {
                            _logger.LogCritical("Failed to deserialize object" + message);
                        }
                        var userToBeDeleted = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == deletedUser.Id);
                        if (userToBeDeleted == default)
                        {
                            _logger.LogError("User already has been deleted" + "Error occured:"+ DateTime.Now);
                        }
                        else
                        {
                            _dbContext.Remove(userToBeDeleted);
                            await _dbContext.SaveChangesAsync();
                        }
                        break;
                    default:
                        break;
                }

                
                await Task.Delay(1000);
            };

            _channel.BasicConsume(queue: "user-service-queue",
                                  autoAck: true,
                                  consumer: consumer);

            await Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _connection.Close();

            _logger.LogInformation("RabbitMQWorker stopped");

            return base.StopAsync(cancellationToken);
        }
    }

}
