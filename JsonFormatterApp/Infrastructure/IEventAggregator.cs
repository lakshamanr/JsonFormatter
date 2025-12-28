using System;

namespace JsonFormatterApp.Infrastructure
{
    /// <summary>
    /// Simple event aggregator for ViewModel communication
    /// </summary>
    public interface IEventAggregator
    {
        void Publish<TMessage>(TMessage message) where TMessage : class;
        void Subscribe<TMessage>(Action<TMessage> handler) where TMessage : class;
        void Unsubscribe<TMessage>(Action<TMessage> handler) where TMessage : class;
    }
}
