using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace UserService.Consumer
{
    public class MailConsumer
    {
        public const string EXCHANGE_NAME = "Kwetter";

        public Task Boot(IModel _connection, string queueName )
        {
            using var channel = _connection;
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
