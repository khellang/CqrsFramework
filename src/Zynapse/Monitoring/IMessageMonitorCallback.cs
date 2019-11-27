using System;

namespace Zynapse.Monitoring
{
    public interface IMessageMonitorCallback
    {
        void ReportSuccess();

        void ReportFailure(Exception error);

        void ReportIgnored();
    }
}
