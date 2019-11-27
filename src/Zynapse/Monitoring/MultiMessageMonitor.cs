using System;
using System.Collections.Generic;
using Zynapse.Messaging;

namespace Zynapse.Monitoring
{
    public sealed class MultiMessageMonitor<TMessage> : IMessageMonitor<TMessage> where TMessage : IMessage
    {
        public MultiMessageMonitor(IReadOnlyCollection<IMessageMonitor<TMessage>> monitors)
        {
            Monitors = monitors;
        }

        private IReadOnlyCollection<IMessageMonitor<TMessage>> Monitors { get; }

        public IMessageMonitorCallback OnMessageIngested(TMessage message)
        {
            var callbacks = new List<IMessageMonitorCallback>(Monitors.Count);

            foreach (var monitor in Monitors)
            {
                callbacks.Add(monitor.OnMessageIngested(message));
            }

            return new Callback(callbacks);
        }

        private sealed class Callback : IMessageMonitorCallback
        {
            public Callback(IReadOnlyCollection<IMessageMonitorCallback> callbacks)
            {
                Callbacks = callbacks;
            }

            private IReadOnlyCollection<IMessageMonitorCallback> Callbacks { get; }

            public void ReportSuccess()
            {
                foreach (var callback in Callbacks)
                {
                    callback.ReportSuccess();
                }
            }

            public void ReportFailure(Exception error)
            {
                foreach (var callback in Callbacks)
                {
                    callback.ReportFailure(error);
                }
            }

            public void ReportIgnored()
            {
                foreach (var callback in Callbacks)
                {
                    callback.ReportIgnored();
                }
            }
        }
    }
}
