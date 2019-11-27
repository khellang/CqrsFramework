using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zynapse.Messaging;
using Zynapse.Monitoring;

namespace Zynapse.EventHandling
{
    public class DefaultEventBus : IEventBus
    {
        public DefaultEventBus(IMessageMonitor<IEventMessage> monitor)
        {
            Monitor = monitor;
            Targets = new HashSet<IMessageTarget<IEventMessage>>();
            Interceptors = new HashSet<IMessageDispatchInterceptor<IEventMessage>>();
        }

        private IMessageMonitor<IEventMessage> Monitor { get; }

        private HashSet<IMessageTarget<IEventMessage>> Targets { get; }

        private HashSet<IMessageDispatchInterceptor<IEventMessage>> Interceptors { get; }

        public Registration Subscribe(IMessageTarget<IEventMessage> target)
        {
            Targets.Add(target);
            return new DelegateRegistration(() => Targets.Remove(target));
        }

        public Registration RegisterInterceptor(IMessageDispatchInterceptor<IEventMessage> interceptor)
        {
            Interceptors.Add(interceptor);
            return new DelegateRegistration(() => Interceptors.Remove(interceptor));
        }

        public async ValueTask Publish(IReadOnlyCollection<IEventMessage> messages, CancellationToken cancellationToken)
        {
            var callbacks = Ingest(messages);

            try
            {
                var intercepted = Intercept(messages);

                await Commit(intercepted, cancellationToken);

                foreach (var handler in Targets)
                {
                    await handler.Handle(intercepted, cancellationToken);
                }

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

        protected virtual ValueTask Commit(IReadOnlyCollection<IEventMessage> messages, CancellationToken cancellationToken)
        {
            return default;
        }

        private IReadOnlyCollection<IMessageMonitorCallback> Ingest(IReadOnlyCollection<IEventMessage> messages)
        {
            return messages.Select(Monitor.OnMessageIngested).ToList();
        }

        private IReadOnlyCollection<IEventMessage> Intercept(IReadOnlyCollection<IEventMessage> messages)
        {
            var processedEvents = new List<IEventMessage>(messages);

            foreach (var interceptor in Interceptors)
            {
                var transform = interceptor.Handle(processedEvents);

                for (var i = 0; i < processedEvents.Count; i++)
                {
                    processedEvents[i] = transform.Invoke(i, processedEvents[i]);
                }
            }

            return processedEvents;
        }
    }
}
