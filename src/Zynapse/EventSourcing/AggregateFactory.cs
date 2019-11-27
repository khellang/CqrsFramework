using Zynapse.Messaging;

namespace Zynapse.EventSourcing
{
    public abstract class AggregateFactory<T> : IAggregateFactory<T>
    {
        T IAggregateFactory<T>.CreateAggregateRoot(string identifier, IAggregateEventMessage @event)
        {
            return ProcessInstance(CreateAggregate(identifier, @event));
        }

        private T CreateAggregate(string identifier, IAggregateEventMessage @event)
        {
            // Check if payload is an aggregate snapshot.
            if (typeof(T).IsAssignableFrom(@event.PayloadType))
            {
                return (T) @event.Payload;
            }

            return CreateAggregateRoot(identifier, @event);
        }

        protected abstract T CreateAggregateRoot(string identifier, IAggregateEventMessage @event);

        protected virtual T ProcessInstance(T aggregate)
        {
            return aggregate;
        }
    }
}
