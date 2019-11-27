using Zynapse.Messaging;

namespace Zynapse.Monitoring
{
    public interface IMessageMonitor<in TMessage> where TMessage : IMessage
    {
        IMessageMonitorCallback OnMessageIngested(TMessage message);
    }
}
