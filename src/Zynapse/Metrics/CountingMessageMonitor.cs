using System;
using System.Diagnostics.Tracing;
using System.Threading;
using Zynapse.Messaging;
using Zynapse.Monitoring;

namespace Zynapse.Metrics
{
    public sealed class CountingMessageMonitor : EventSource, IMessageMonitor<IMessage>, IMessageMonitorCallback
    {
        public static readonly CountingMessageMonitor Instance = new CountingMessageMonitor();

        private CountingMessageMonitor() : base("Zynapse", EventSourceSettings.EtwSelfDescribingEventFormat)
        {
        }

        private IncrementingPollingCounter? _ingestRateCounter;
        private IncrementingPollingCounter? _processRateCounter;
        private IncrementingPollingCounter? _failureRateCounter;

        private PollingCounter? _ingestedCounter;
        private PollingCounter? _processedCounter;
        private PollingCounter? _successCounter;
        private PollingCounter? _failureCounter;
        private PollingCounter? _ignoredCounter;

        private long _ingested;
        private long _processed;
        private long _success;
        private long _failure;
        private long _ignored;

        public IMessageMonitorCallback OnMessageIngested(IMessage message)
        {
            Interlocked.Increment(ref _ingested);
            return this;
        }

        public void ReportSuccess()
        {
            Interlocked.Increment(ref _processed);
            Interlocked.Increment(ref _success);
        }

        public void ReportFailure(Exception error)
        {
            Interlocked.Increment(ref _processed);
            Interlocked.Increment(ref _failure);
        }

        public void ReportIgnored()
        {
            Interlocked.Increment(ref _ignored);
        }

        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            if (command.Command != EventCommand.Enable)
            {
                return;
            }

            _ingestRateCounter = new IncrementingPollingCounter("ingestion-rate", this, () => _ingested)
            {
                DisplayName = "Ingested"
            };

            _processRateCounter = new IncrementingPollingCounter("processing-rate", this, () => _ingested)
            {
                DisplayName = "Processed"
            };

            _failureRateCounter = new IncrementingPollingCounter("failure-rate", this, () => _failure)
            {
                DisplayName = "Failed"
            };

            _ingestedCounter = new PollingCounter("ingested", this, () => _ingested)
            {
                DisplayName = "Ingested"
            };

            _processedCounter = new PollingCounter("processed", this, () => _ingested)
            {
                DisplayName = "Processed"
            };

            _successCounter = new PollingCounter("success", this, () => _success)
            {
                DisplayName = "Success"
            };

            _failureCounter = new PollingCounter("failed", this, () => _failure)
            {
                DisplayName = "Failed"
            };

            _ignoredCounter = new PollingCounter("ignored", this, () => _ignored)
            {
                DisplayName = "Ignored"
            };
        }
    }
}
