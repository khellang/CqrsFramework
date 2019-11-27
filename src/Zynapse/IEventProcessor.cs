using System.Threading;
using System.Threading.Tasks;
using Zynapse.Messaging;

namespace Zynapse
{
    public interface IEventProcessor : IMessageTarget<IEventMessage>, IMessageHandler<IEventMessage>
    {
        void Start();

        ValueTask Stop(CancellationToken cancellationToken);
    }
}