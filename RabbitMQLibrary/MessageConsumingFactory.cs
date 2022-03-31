using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQLibrary
{
    public class MessageConsumingFactory: IDisposable
    {
        public Task AddMessageConsuming(string queueName)
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
                    return Task.CompletedTask;
                }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
