using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace RabbitMQLibrary
{
    /// <summary>
    /// Singleton service keeping track of our connection with RabbitMQ.
    /// </summary>
    internal class RabbitMqConnection : IDisposable
    {
        private IConnection _connection;
        public IModel CreateChannel()
        {
            var connection = GetConnection();
            return connection.CreateModel();
        }

        private IConnection GetConnection()
        {
            if (_connection == null)
            {
                var factory = new ConnectionFactory
                {
                    UserName = "guest",
                    Password = "guest",
                    Port = AmqpTcpEndpoint.UseDefaultPort,
                    AutomaticRecoveryEnabled = true
                };
                var endpoints = new List<AmqpTcpEndpoint>
                {
                          new AmqpTcpEndpoint("rabbitmq"),
                          new AmqpTcpEndpoint("localhost"),
                          new AmqpTcpEndpoint("production-rabbitmqcluster")
                };
                _connection = factory.CreateConnection(endpoints);
            }

            return _connection;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
