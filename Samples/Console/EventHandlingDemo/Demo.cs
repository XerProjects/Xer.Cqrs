using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SimpleInjector;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Attributes;
using Xer.Cqrs.EventStack.Publishers;
using Xer.Cqrs.EventStack.Registrations;
using Xer.Cqrs.EventStack.Resolvers;

namespace Console.EventHandlingDemo
{
    public class Demo
    {
        private EventPublisher _eventPublisher;

        public Demo()
        {
            _eventPublisher = SetupEventPublisher();
        }

        private EventPublisher SetupEventPublisher()
        {
            // Register all event handlers to a container of your choice.
            var container = new Container();
            var assemblyContainerEventHandlers = Assembly.GetExecutingAssembly();
            container.RegisterCollection(typeof(IEventHandler<>), assemblyContainerEventHandlers);
            container.RegisterCollection(typeof(IEventAsyncHandler<>), assemblyContainerEventHandlers);
            
            // Enable EventPuclisher to resolve event handlers from the container 
            // by creating an implementation of IContainerAdapter and passing it to the 
            // ContainerEventHandlerResolver which can then used by the dispatcher.
            var containerResolver = new ContainerEventHandlerResolver(new SimpleInjectorContainerAdapter(container));

            // Register any methods marked with [EventHandler] 
            // which will be invoked when resolved by the EventPublisher.
            var attributeRegistration = new EventHandlerAttributeRegistration();
            attributeRegistration.Register(() => new SampleEventHandlerAttributeHandler());
            
            // Register any implementations of IEventAsyncHandler/IEventHandler
            // which will be invoked when resolved by the EventPublisher.
            var basicRegistration = new EventHandlerRegistration();
            basicRegistration.Register<SampleEvent>(() => new SampleEventHandler());
            basicRegistration.Register<SampleEvent>(() => new SampleEventAsyncHandler());

            // Combine multiple sources of event handlers into a CompositeEventHandlerResolver.
            // This will try to resolve al event handlers from the provided list of resolvers.
            return new EventPublisher(new CompositeEventHandlerResolver(new IEventHandlerResolver[] 
            {
                containerResolver,
                attributeRegistration,
                basicRegistration
            }));
        }

        public async Task ExecuteDemoAsync()
        {
            // Dispatch event to all registered event handlers.
            await _eventPublisher.PublishAsync(new SampleEvent());
        }
    }

    #region Events
        
    class SampleEvent : IEvent
    {

    }

    class SampleEventHandler : IEventHandler<SampleEvent>
    {
        public void Handle(SampleEvent @event)
        {
            System.Console.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event.");
        }
    }

    class SampleEventAsyncHandler : IEventAsyncHandler<SampleEvent>
    {
        public Task HandleAsync(SampleEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            System.Console.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event.");
            return Task.CompletedTask;
        }
    }

    class SampleEventHandlerAttributeHandler
    {
        [EventHandler]
        public Task HandleSampleEventAsync(SampleEvent @event, CancellationToken cancellationToken)
        {
            System.Console.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event asynchronously.");
            return Task.CompletedTask;
        }

        [EventHandler]
        public void HandleSampleEvent(SampleEvent @event)
        {
            System.Console.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event synchronously.");
        }
    }

    #endregion Events
}