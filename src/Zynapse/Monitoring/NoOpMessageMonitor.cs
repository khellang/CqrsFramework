using System;
using Zynapse.Messaging;

namespace Zynapse.Monitoring
{
    public sealed class NoOpMessageMonitor : IMessageMonitor<IMessage>, IMessageMonitorCallback
    {
        public static readonly NoOpMessageMonitor Instance = new NoOpMessageMonitor();

        private NoOpMessageMonitor() { }

        public IMessageMonitorCallback OnMessageIngested(IMessage message) => this;

        public void ReportSuccess() { }

        public void ReportFailure(Exception error) { }

        public void ReportIgnored() { }
    }
}
