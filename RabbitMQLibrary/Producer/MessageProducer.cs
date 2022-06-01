using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RabbitMQLibrary
{
    internal class MessageProducer : IMessageProducer
    {
        private readonly string _exchangeName;
        private readonly RabbitMqConnection _connection;

        private readonly ILogger<MessageProducer> _logger;

        public MessageProducer(ILogger<MessageProducer> logger, ExchangeName exchangeName, RabbitMqConnection connection)
        {
            _exchangeName = exchangeName.Name;
            _logger = logger;
            _connection = connection;
        }

        public void PublishMessageAsync<T>(string routingKey, T message)
        {
            _logger.LogInformation($"Sending message {message} with routing key {routingKey}");

            using (IModel channel = _connection.CreateChannel())
            {
                channel.ExchangeDeclare(exchange: _exchangeName,
                                        durable: true,
                                        type: ExchangeType.Topic);

                string messageBody = JsonConvert.SerializeObject(message);
                byte[] body = Encoding.UTF8.GetBytes(messageBody);

                channel.BasicPublish(exchange: _exchangeName,
                                     routingKey: routingKey,
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}
