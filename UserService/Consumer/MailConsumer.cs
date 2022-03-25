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
            using (var channel = _connection)
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
                return Task.CompletedTask;
            }
        }
    }
}
