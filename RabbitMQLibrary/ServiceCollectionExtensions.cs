using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQLibrary;
using System;
using System.Text;

namespace RabbitMQLibrary
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMessageProducing(this IServiceCollection services, string exchangeName)
        {
            // Add to every service in the microservices the ExchangeName
            var exchangeNameService = new ExchangeName(exchangeName);
            services.AddSingleton(exchangeNameService);
            // Create a new rabbitmq connection and add it to the service collection that way every service has the connection
            var connection = new RabbitMqConnection();
            services.AddSingleton(connection);
            // Give the IMessagePublisher its MessageProducer that way it can start producing messages

            services.AddScoped<IMessageProducer, MessageProducer>();
        }

        public static void AddMessageConsuming(string queueName)
        {
            // Create a new rabbitmq connection and add it to the service collection that way every service has the connection
            var connection = new RabbitMqConnection();
            using (var channel = connection.CreateChannel())
            {
                channel.QueueDeclare(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
