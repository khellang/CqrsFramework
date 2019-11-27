using System;
using System.Collections.Immutable;
using NodaTime;

namespace Zynapse.Messaging
{
    public interface IMessage
    {
        string Identifier { get; }

        object? Payload { get; }

        Type PayloadType { get; }

        MessageMetadata Metadata { get; }
    }

    public abstract class Message<TPayload> : IMessage
    {
        protected Message(string identifier, TPayload payload, MessageMetadata metadata)
        {
            Identifier = identifier;
            Payload = payload;
            Metadata = metadata;
        }

        public string Identifier { get; }

        public TPayload Payload { get; }

        object? IMessage.Payload => Payload;

        Type IMessage.PayloadType => typeof(TPayload);

        public MessageMetadata Metadata { get; }
    }

    public interface IEventMessage : IMessage
    {
        Instant Timestamp { get; }
    }

    public class EventMessage<TPayload> : Message<TPayload>, IEventMessage
    {
        public EventMessage(string identifier, TPayload payload, MessageMetadata metadata, Instant timestamp)
            : base(identifier, payload, metadata)
        {
            Timestamp = timestamp;
        }

        private EventMessage(IEventMessage message)
            : base(message.Identifier, (TPayload)message.Payload, message.Metadata)
        {
            Timestamp = message.Timestamp;
        }

        public Instant Timestamp { get; }

        public static EventMessage<TPayload> Downcast(IEventMessage message)
        {
            if (message is EventMessage<TPayload> eventMessage)
            {
                return eventMessage;
            }

            return new EventMessage<TPayload>(message);
        }
    }

    public interface IAggregateEventMessage : IEventMessage
    {
        string AggregateIdentifier { get; }

        Type AggregateType { get; }

        long SequenceNumber { get; }
    }

    public class AggregateEventMessage<TAggregate, TPayload> : EventMessage<TPayload>, IAggregateEventMessage
    {
        public AggregateEventMessage(string identifier, TPayload payload, MessageMetadata metadata, Instant timestamp, string aggregateIdentifier, long sequenceNumber)
            : base(identifier, payload, metadata, timestamp)
        {
            AggregateIdentifier = aggregateIdentifier;
            SequenceNumber = sequenceNumber;
        }

        public string AggregateIdentifier { get; }

        Type IAggregateEventMessage.AggregateType => typeof(TAggregate);

        public long SequenceNumber { get; }
    }

    public interface ICommandMessage : IMessage
    {
    }

    public class MessageMetadata
    {
        public static readonly MessageMetadata Empty = new MessageMetadata(ImmutableDictionary<string, object>.Empty);

        private MessageMetadata(IImmutableDictionary<string, object> values)
        {
            Values = values;
        }

        private IImmutableDictionary<string, object> Values { get; }
    }
}