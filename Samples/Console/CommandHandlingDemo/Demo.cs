using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleInjector;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Attributes;
using Xer.Cqrs.CommandStack.Dispatchers;
using Xer.Cqrs.CommandStack.Registrations;
using Xer.Cqrs.CommandStack.Resolvers;

namespace Console.CommandHandlingDemo
{
    public class Demo
    {
        private CommandDispatcher _dispatcherWithContainerRegistration;
        private CommandDispatcher _dispatcherWithAttributeRegistration;
        private CommandDispatcher _dispatcherWithBasicRegistration;
        
        public Demo()
        {
            _dispatcherWithContainerRegistration = SetupDispatcherWithContainerRegistration();
            _dispatcherWithAttributeRegistration = SetupDispatcherWithAttributeRegistration();
            _dispatcherWithBasicRegistration = SetupDispatcherWithBasicRegistration();
        }

        private CommandDispatcher SetupDispatcherWithContainerRegistration()
        {
            // Register any command handlers to a container of your choice.
            var container = new Container();
            container.Register<ICommandHandler<SampleCommand>, SampleCommandHandler>();

            // Enable CommmandDispatcher to resolve comand handlers from the container 
            // by creating an implementation of IContainerAdapter and passing it to the 
            // ContainerCommandHandlerResolver which can then used by the dispatcher.
            return new CommandDispatcher(new ContainerCommandHandlerResolver(
                                            new SimpleInjectorContainerAdapter(container)));
        }

        private CommandDispatcher SetupDispatcherWithAttributeRegistration()
        {
            // Register any methods marked with [CommandHandler] 
            // which will be invoked when resolved by the CommandDispatcher.
            var attributeRegistration = new CommandHandlerAttributeRegistration();
            attributeRegistration.Register(() => new SampleCommandHandlerAttributeHandler());
            
            return new CommandDispatcher(attributeRegistration);
        }

        private CommandDispatcher SetupDispatcherWithBasicRegistration()
        {
            // Register any implementations of ICommandAsyncHandler/ICommandHandler
            // which will be invoked when resolved by the CommandDispatcher.
            var registration = new CommandHandlerRegistration();
            registration.Register<SampleCommand>(() => new SampleCommandAsyncHandler());

            return new CommandDispatcher(registration);
        }

        public async Task ExecuteDemoAsync()
        {
            // Dispatch commands to the registered command handler.
            await _dispatcherWithContainerRegistration.DispatchAsync(new SampleCommand());
            await _dispatcherWithAttributeRegistration.DispatchAsync(new SampleCommand());
            await _dispatcherWithBasicRegistration.DispatchAsync(new SampleCommand());
        }
    }
    #region Commands
    
    class SampleCommand : Command
    {

    }

    class SampleCommandHandler : ICommandHandler<SampleCommand>
    {
        public void Handle(SampleCommand command)
        {
            System.Console.WriteLine($"{GetType().Name} handled {command.GetType().Name} command.");
        }
    }

    class SampleCommandAsyncHandler : ICommandAsyncHandler<SampleCommand>
    {
        public Task HandleAsync(SampleCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            System.Console.WriteLine($"{GetType().Name} handled {command.GetType().Name} command.");
            return Task.CompletedTask;
        }
    }

    class SampleCommandHandlerAttributeHandler
    {
        [CommandHandler]
        public Task HandleSampleCommandAsync(SampleCommand command, CancellationToken cancellationToken)
        {
            System.Console.WriteLine($"{GetType().Name} handled {command.GetType().Name} command.");
            return Task.CompletedTask;
        }
    }

    #endregion Commands
}