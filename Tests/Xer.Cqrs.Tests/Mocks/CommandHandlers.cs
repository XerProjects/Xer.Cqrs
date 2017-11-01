using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Attributes;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Mocks
{
    #region Base Command Handler
    
    public abstract class TestCommandHandlerBase
    {
        private List<ICommand> _handledCommands = new List<ICommand>();

        protected ITestOutputHelper TestOutputHelper { get; }

        public IReadOnlyCollection<ICommand> HandledCommands => _handledCommands.AsReadOnly();

        public TestCommandHandlerBase(ITestOutputHelper outputHelper)
        {
            TestOutputHelper = outputHelper;
        }

        protected void HandleAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            TestOutputHelper.WriteLine($"{DateTime.Now}: {GetType().Name} executed command of type {command.GetType().Name} asynchronously.");
            _handledCommands.Add(command);
        }

        protected void Handle<TCommand>(TCommand command) where TCommand : ICommand
        {
            TestOutputHelper.WriteLine($"{DateTime.Now}: {GetType().Name} executed command of type {command.GetType().Name}.");
            _handledCommands.Add(command);
        }
    }

    #endregion Base Command Handler

    #region Command Handlers

    public class TestCommandHandler : TestCommandHandlerBase,
                                      ICommandAsyncHandler<DoSomethingCommand>,
                                      ICommandAsyncHandler<DoSomethingAsyncCommand>,
                                      ICommandAsyncHandler<DoSomethingAsyncWithCancellationCommand>,
                                      ICommandAsyncHandler<DoSomethingAsyncForSpecifiedDurationCommand>,
                                      ICommandAsyncHandler<ThrowExceptionCommand>,
                                      ICommandHandler<DoSomethingCommand>,
                                      ICommandHandler<DoSomethingAsyncCommand>,
                                      ICommandHandler<DoSomethingAsyncWithCancellationCommand>,
                                      ICommandHandler<DoSomethingAsyncForSpecifiedDurationCommand>,
                                      ICommandHandler<ThrowExceptionCommand>
    {
        public TestCommandHandler(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
        
        public void Handle(DoSomethingCommand command)
        {
            base.Handle(command);
        }

        public void Handle(DoSomethingAsyncCommand command)
        {
            base.Handle(command);
        }

        public void Handle(DoSomethingAsyncWithCancellationCommand command)
        {
            base.Handle(command);
        }

        public void Handle(DoSomethingAsyncForSpecifiedDurationCommand command)
        {
            base.Handle(command);
        }

        public void Handle(ThrowExceptionCommand command)
        {
            base.Handle(command);
            throw new TestCommandHandlerException("This is a triggered post-processing exception.");
        }

        public Task HandleAsync(DoSomethingCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            base.HandleAsync(command);
            return Task.CompletedTask;
        }

        public Task HandleAsync(DoSomethingAsyncCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            base.HandleAsync(command);
            return Task.CompletedTask;
        }

        public Task HandleAsync(DoSomethingAsyncWithCancellationCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            base.HandleAsync(command);
            return Task.CompletedTask;
        }

        public Task HandleAsync(DoSomethingAsyncForSpecifiedDurationCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            base.HandleAsync(command);
            return Task.CompletedTask;
        }

        public Task HandleAsync(ThrowExceptionCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            base.HandleAsync(command);
            return Task.FromException(new TestCommandHandlerException("This is a triggered post-processing exception."));
        }
    }

    #endregion Command Handlers

    #region Attributed Command Handlers

    public class TestAttributedCommandHandler : TestCommandHandlerBase
    {

        public TestAttributedCommandHandler(ITestOutputHelper output)
            : base(output)
        {
        }

        [CommandHandler]
        public void DoSomething(DoSomethingCommand command)
        {
            base.Handle(command);
        }

        [CommandHandler]
        public void DoSomethingWithException(ThrowExceptionCommand command)
        {
            base.Handle(command);
            throw new TestCommandHandlerException("This is a triggered post-processing exception.");
        }

        [CommandHandler]
        public Task DoSomethingAsync(DoSomethingAsyncCommand command)
        {
            base.HandleAsync(command);
            return Task.CompletedTask;
        }

        [CommandHandler]
        public Task DoSomethingAsync(DoSomethingAsyncWithCancellationCommand command, CancellationToken ctx)
        {
            if(ctx == null)
            {
                return Task.FromException(new TestCommandHandlerException("Cancellation token is null. Please check attribute registration."));
            }

            base.HandleAsync(command);
            return Task.CompletedTask;
        }

        [CommandHandler]
        public async Task DoSomethingAsync(DoSomethingAsyncForSpecifiedDurationCommand command, CancellationToken ctx)
        {
            await Task.Delay(command.DurationInMilliSeconds, ctx);

            base.Handle(command);
        }
    }

    public class TestAttributedCommandHandlerWithAsyncVoid : TestCommandHandlerBase
    {
        public TestAttributedCommandHandlerWithAsyncVoid(ITestOutputHelper output)
            : base(output)
        {
        }

        [CommandHandler]
        public async void DoAsyncVoidHandlerCommand(DoAsyncVoidHandlerCommand command)
        {
            HandleAsync(command);

            await Task.Yield();
        }
    }

    public class TestAttributedSyncCommandHandlerWithCancellationToken : TestCommandHandlerBase
    {
        public TestAttributedSyncCommandHandlerWithCancellationToken(ITestOutputHelper output)
            : base(output)
        {
        }

        [CommandHandler]
        public void Handle(DoSomethingCommand command, CancellationToken cancellationToken)
        {
            base.Handle(command);
        }
    }

    #endregion Attributed Command Handlers
    
    public class TestCommandHandlerException : Exception
    {
        public TestCommandHandlerException() { }
        public TestCommandHandlerException(string message) : base(message) { }
    }
}
