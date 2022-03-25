using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RabbitMQLibrary
{
    internal class MessageConsumer: IMessageConsumer
    {
        public const string EXCHANGE_NAME = "Kwetter";
        private readonly RabbitMqConnection _connection;

        public MessageConsumer(RabbitMqConnection connection)
        {
            _connection = connection;
        }

        public Task ConsumeMessageAsync(string queueName,string messageType)
        {
            using var channel = _connection.CreateChannel();
            channel.ExchangeDeclare(EXCHANGE_NAME, "fanout", durable: true);
            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueBind(queueName, EXCHANGE_NAME, routingKey: string.Empty);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);
            };
            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }
    }
}
