using System;

namespace Foundation.WebSocket
{
    public class ReconnectStrategy
    {
        private readonly int _reconnectStepInterval;

        private readonly int? _initMaxAttempts;

        private int _minReconnectInterval;

        private readonly int _maxReconnectInterval;

        private int? _maxAttempts;

        private int _attemptsMade;

        public ReconnectStrategy()
        {
            _reconnectStepInterval = 3000;
            _minReconnectInterval = 3000;
            _maxReconnectInterval = 30000;
            _maxAttempts = null; //вечно
            _initMaxAttempts = null;
            _attemptsMade = 0;
        }

        public ReconnectStrategy(int minReconnectInterval, int maxReconnectInterval, int? maxAttempts)
        {
            _reconnectStepInterval = minReconnectInterval;
            _minReconnectInterval = Math.Min(minReconnectInterval, maxReconnectInterval);
            _maxReconnectInterval = Math.Max(maxReconnectInterval, minReconnectInterval);

            _maxAttempts = maxAttempts;
            _initMaxAttempts = maxAttempts;
            _attemptsMade = 0;
        }

        public ReconnectStrategy(int minReconnectInterval, int maxReconnectInterval)
        {
            _reconnectStepInterval = minReconnectInterval;
            _minReconnectInterval = Math.Min(minReconnectInterval, maxReconnectInterval);
            _maxReconnectInterval = Math.Max(maxReconnectInterval, minReconnectInterval);

            _maxAttempts = null;
            _initMaxAttempts = null;
            _attemptsMade = 0;
        }

        public ReconnectStrategy(int reconnectInterval)
        {
            _reconnectStepInterval = reconnectInterval;
            _minReconnectInterval = reconnectInterval;
            _maxReconnectInterval = reconnectInterval;
            _maxAttempts = null;
            _initMaxAttempts = null;
            _attemptsMade = 0;
        }

        public ReconnectStrategy(int reconnectInterval, int? maxAttempts)
        {
            _reconnectStepInterval = reconnectInterval;
            _minReconnectInterval = reconnectInterval;
            _maxReconnectInterval = reconnectInterval;
            _maxAttempts = maxAttempts;
            _initMaxAttempts = maxAttempts;
            _attemptsMade = 0;
        }

        public void SetMaxAttempts(int attempts)
        {
            _maxAttempts = attempts;
        }

        public void Reset()
        {
            _attemptsMade = 0;
            _minReconnectInterval = _reconnectStepInterval;
            _maxAttempts = _initMaxAttempts;
        }

        public void SetAttemptsMade(int count) => _attemptsMade = count;

        internal void Increment()
        {
            _attemptsMade++;
            if (_minReconnectInterval < _maxReconnectInterval)
                _minReconnectInterval += _reconnectStepInterval;
            if (_minReconnectInterval > _maxReconnectInterval)
                _minReconnectInterval = _maxReconnectInterval;
        }

        public int GetReconnectInterval() => _minReconnectInterval;

        public bool AreAttemptsComplete() => _attemptsMade == _maxAttempts;
    }
}
