using Microsoft.Extensions.Logging;
using Middleware.Shared.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Middleware.Shared.Services
{
    // https://github.com/isanka88/Send-and-Receive-Data-in-Asp.net-Core-With-RabbitMQ
    public class RabbitMQPersistentConnection
    {
        private readonly ILogger<RabbitMQPersistentConnection> _logger;
        private readonly IConnectionFactory _connectionFactory;
        IConnection? _connection;
        EventBus? _eventBus;
        bool _disposed;

        public RabbitMQPersistentConnection(ILogger<RabbitMQPersistentConnection> logger, IConnectionFactory connectionFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            if (!IsConnected) {
                TryConnect();
            }
        }

        public bool IsConnected
        {
            get {
                return _connection is not null && _connection.IsOpen && !_disposed;
            }
        }

        public bool TryConnect()
        {
            try {
                _logger.LogInformation("RabbitMQ Client is trying to connect.");
                _connection = _connectionFactory.CreateConnection();
            } catch (BrokerUnreachableException e) {
                Thread.Sleep(5000);
                _logger.LogInformation(e.Message);
                _connection = _connectionFactory.CreateConnection();
            }

            if (IsConnected) {
                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.CallbackException += OnConnectionException;
                _connection.ConnectionBlocked += OnConnectionBlocked;

                _logger.LogInformation($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");

                return true;
            } else {
                _logger.LogError("RabbitMQ connection could not be created.");
                return false;
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected) {
                throw new InvalidOperationException("No RabbitMQ connection is available to created a model.");
            }

            return _connection!.CreateModel();
        }

        public void CreateConsumerChannel()
        {
            if (!IsConnected) {
                TryConnect();
            }

            foreach (ConsumingQueueName qn in Enum.GetValues(typeof(ConsumingQueueName))) {
                _eventBus = new EventBus(this, qn);
                _eventBus.CreateConsumerChannel();
            }
        }

        public void Disconnect()
        {
            if (_disposed) {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;

            try {
                _connection.Dispose();
                 _disposed = true;
            } catch (IOException ex) {
                _logger.LogError(ex.ToString());
            }
        }

        private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;
            _logger.LogError("A RabbitMQ connection is blocked. Trying to re-connect...");
            TryConnect();
        }

        private void OnConnectionException(object? sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;
            _logger.LogError("A RabbitMQ connection threw an exception. Trying to re-connect...");
            TryConnect();
        }

        private void OnConnectionShutdown(object? sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;
            _logger.LogError("A RabbitMQ connection was destroyed. Trying to re-connect...");
            TryConnect();
        }
    }
}
