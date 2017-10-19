using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xer.Cqrs.Events.Resolvers
{
    public class ContainerEventHandlerResolver : IEventHandlerResolver
    {
        private readonly IContainerAdapter _containerAdapter;
        private static readonly Task _completedTask = Task.FromResult(0);

        public ContainerEventHandlerResolver(IContainerAdapter containerAdapter)
        {
            _containerAdapter = containerAdapter;
        }

        public EventHandlerDelegate ResolveEventHandler<TEvent>() where TEvent : IEvent
        {
            IEnumerable<IEventAsyncHandler<TEvent>> asyncEventHandlers = _containerAdapter.ResolveMultiple<IEventAsyncHandler<TEvent>>();

            IEnumerable<IEventHandler<TEvent>> syncEventHandlers = _containerAdapter.ResolveMultiple<IEventHandler<TEvent>>();

            return new EventHandlerDelegate((e, ct) =>
            {
                var asyncEventHandlerTasks = asyncEventHandlers?.Select(eventHandler => eventHandler.HandleAsync((TEvent)e, ct));
                var syncHandlerTasks = syncEventHandlers?.Select(eventHandler =>
                {
                    eventHandler.Handle((TEvent)e);
                    return _completedTask;
                });

                return Task.WhenAll(asyncEventHandlerTasks.Concat(syncHandlerTasks));
            });
        }
    }

    public interface IContainerAdapter
    {
        IEnumerable<T> ResolveMultiple<T>();
    }
}
