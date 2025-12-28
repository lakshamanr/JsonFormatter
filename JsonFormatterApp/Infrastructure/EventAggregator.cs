using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonFormatterApp.Infrastructure
{
    /// <summary>
    /// Simple event aggregator implementation for loosely-coupled ViewModel communication
    /// </summary>
    public class EventAggregator : IEventAggregator
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public void Publish<TMessage>(TMessage message) where TMessage : class
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var messageType = typeof(TMessage);

            if (_subscribers.ContainsKey(messageType))
            {
                var handlers = _subscribers[messageType].ToList();
                foreach (var handler in handlers)
                {
                    ((Action<TMessage>)handler)(message);
                }
            }
        }

        public void Subscribe<TMessage>(Action<TMessage> handler) where TMessage : class
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var messageType = typeof(TMessage);

            if (!_subscribers.ContainsKey(messageType))
            {
                _subscribers[messageType] = new List<Delegate>();
            }

            _subscribers[messageType].Add(handler);
        }

        public void Unsubscribe<TMessage>(Action<TMessage> handler) where TMessage : class
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var messageType = typeof(TMessage);

            if (_subscribers.ContainsKey(messageType))
            {
                _subscribers[messageType].Remove(handler);
            }
        }
    }
}
