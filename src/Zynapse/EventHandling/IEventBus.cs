using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zynapse.Messaging;

namespace Zynapse.EventHandling
{
    public interface IEventBus : IMessageSource<IEventMessage>, IMessageDispatcher<IEventMessage>
    {
        ValueTask Publish(IReadOnlyCollection<IEventMessage> messages, CancellationToken cancellationToken);
    }
}