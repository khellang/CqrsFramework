using System;
using System.Reflection;
using Zynapse.Messaging;

namespace Zynapse.EventSourcing
{
    public interface IAggregateFactory<out T>
    {
        T CreateAggregateRoot(string identifier, IAggregateEventMessage firstEvent);
    }

    public class DefaultAggregateFactory<T> : AggregateFactory<T>
    {
        public DefaultAggregateFactory()
        {
            Constructor = GetFactoryConstructor(typeof(T));
        }

        private ConstructorInfo? Constructor { get; }

        protected override T CreateAggregateRoot(string identifier, IAggregateEventMessage @event)
        {
            throw new System.NotImplementedException();
        }

        private static ConstructorInfo? GetFactoryConstructor(Type type)
        {
            return type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
        }
    }
}
