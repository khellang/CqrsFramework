using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Zynapse.Common;
using Zynapse.EventHandling;
using Zynapse.Messaging;
using Zynapse.Metrics;
using Zynapse.Monitoring;

namespace Zynapse
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var bus = new DefaultEventBus(CountingMessageMonitor.Instance);

            await using var sub = bus.Subscribe(new EventMessageHandler());

            var clock = SystemClock.Instance;
            var generator = DefaultIdentifierGenerator.Instance;

            var aggregateIdentifier = Guid.NewGuid().ToString();

            for (var i = 0; i < 10000; i++)
            {
                var timestamp = clock.GetCurrentInstant();
                var payload = new TestEvent($"Event #{i}");
                var identifier = generator.GenerateIdentifier();

                var message = new AggregateEventMessage<TestAggregate, TestEvent>(identifier, payload, MessageMetadata.Empty, timestamp, aggregateIdentifier, i);

                try
                {
                    await bus.Publish(new[] { message }, CancellationToken.None);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(20, 200)));
            }
        }
    }

    public class EventMessageHandler : IMessageTarget<IEventMessage>
    {
        private long _counter;

        public ValueTask Handle(IReadOnlyCollection<IEventMessage> messages, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _counter);

            if (_counter % 10 == 0)
            {
                throw new Exception("FAIL!");
            }

            Console.WriteLine($"Received {messages.Count} events:");

            foreach (var message in messages)
            {
                Console.WriteLine(message.Payload);
            }

            Console.WriteLine();
            return default;
        }
    }

    public class TestAggregate
    {
    }

    public class TestEvent
    {
        public TestEvent(string message)
        {
            Message = message;
        }

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }

    public abstract class Registration : IDisposable, IAsyncDisposable
    {
        public abstract void Dispose();

        public virtual ValueTask DisposeAsync()
        {
            Dispose();
            return default;
        }
    }

    public class DelegateRegistration : Registration
    {
        public DelegateRegistration(Action action)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        private Action Action { get; }

        public override void Dispose() => Action.Invoke();
    }

    public interface IMessageDispatchInterceptor<TMessage> where TMessage : IMessage
    {
        Func<int, TMessage, TMessage> Handle(IReadOnlyCollection<TMessage> messages);
    }

    public interface IMessageDispatcher<TMessage> where TMessage : IMessage
    {
        Registration RegisterInterceptor(IMessageDispatchInterceptor<TMessage> interceptor);
    }

    public interface IMessageSource<out TMessage> where TMessage : IMessage
    {
        Registration Subscribe(IMessageTarget<TMessage> handler);
    }

    public interface IMessageHandlerInterceptor<out TMessage> where TMessage : IMessage
    {
    }

    public interface IMessageHandler<in TMessage> where TMessage : IMessage
    {
        Registration RegisterInterceptor(IMessageHandlerInterceptor<TMessage> interceptor);
    }

    public interface IMessageTarget<in TMessage> where TMessage : IMessage
    {
        ValueTask Handle(IReadOnlyCollection<TMessage> messages, CancellationToken cancellationToken);
    }

    public abstract class EventProcessor : IEventProcessor
    {
        protected EventProcessor(IMessageMonitor<IEventMessage> monitor)
        {
            Monitor = monitor;
            Interceptors = new HashSet<IMessageHandlerInterceptor<IEventMessage>>();
        }

        private IMessageMonitor<IEventMessage> Monitor { get; }

        private HashSet<IMessageHandlerInterceptor<IEventMessage>> Interceptors { get; }

        public Registration RegisterInterceptor(IMessageHandlerInterceptor<IEventMessage> interceptor)
        {
            Interceptors.Add(interceptor);
            return new DelegateRegistration(() => Interceptors.Remove(interceptor));
        }

        public async ValueTask Handle(IReadOnlyCollection<IEventMessage> messages, CancellationToken cancellationToken)
        {
            var callbacks = Ingest(messages);

            try
            {
                // TODO: Actually handle the messages.

                foreach (var callback in callbacks)
                {
                    callback.ReportSuccess();
                }
            }
            catch (Exception e)
            {
                foreach (var callback in callbacks)
                {
                    callback.ReportFailure(e);
                }

                throw;
            }
        }

        public abstract void Start();

        public abstract ValueTask Stop(CancellationToken cancellationToken);

        private IReadOnlyCollection<IMessageMonitorCallback> Ingest(IReadOnlyCollection<IEventMessage> messages)
        {
            return messages.Select(Monitor.OnMessageIngested).ToList();
        }
    }

    public class DefaultEventProcessor : EventProcessor
    {
        public DefaultEventProcessor(IMessageMonitor<IEventMessage> monitor, IMessageSource<IEventMessage> messageSource)
            : base(monitor)
        {
            MessageSource = messageSource;
        }

        private IMessageSource<IEventMessage> MessageSource { get; }

        private Registration? Registration { get; set; }

        public override void Start()
        {
            Registration = MessageSource.Subscribe(this);
        }

        public override ValueTask Stop(CancellationToken cancellationToken)
        {
            return Registration?.DisposeAsync() ?? default;
        }
    }
}
