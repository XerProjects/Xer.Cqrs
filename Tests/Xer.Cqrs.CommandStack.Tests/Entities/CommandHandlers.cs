using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Attributes;
using Xunit.Abstractions;

namespace Xer.Cqrs.CommandStack.Tests.Entities
{
    #region Command Handler

    public class TestCommandHandler : ICommandAsyncHandler<TestCommand>,
                                      ICommandAsyncHandler<CancellableTestCommand>,
                                      ICommandAsyncHandler<DelayCommand>,
                                      ICommandAsyncHandler<ThrowExceptionCommand>,
                                      ICommandHandler<TestCommand>,
                                      ICommandHandler<DelayCommand>,
                                      ICommandHandler<ThrowExceptionCommand>
    {
        protected List<object> InternalHandledCommands { get; } = new List<object>();

        protected ITestOutputHelper OutputHelper { get; }

        public IReadOnlyCollection<object> HandledCommands => InternalHandledCommands.AsReadOnly();

        public TestCommandHandler(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        public bool HasHandledCommand<TCommand>()
        {
            return InternalHandledCommands.Any(c => c is TCommand);
        }

        public ICommandAsyncHandler<TCommand> AsCommandAsyncHandler<TCommand>() where TCommand : class
        {
            return this as ICommandAsyncHandler<TCommand>;
        }

        public ICommandHandler<TCommand> AsCommandSyncHandler<TCommand>() where TCommand : class
        {
            return this as ICommandHandler<TCommand>;
        }
        
        public void Handle(TestCommand command)
        {
            BaseHandle(command);
        }

        public void Handle(DelayCommand command)
        {
            Task.Delay(command.DurationInMilliSeconds).GetAwaiter().GetResult();
            BaseHandle(command);
        }

        public void Handle(ThrowExceptionCommand command)
        {
            BaseHandle(command);
            throw new TestCommandHandlerException("This is a triggered exception.");
        }

        public Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            BaseHandle(command);
            return Task.CompletedTask;
        }

        public Task HandleAsync(CancellableTestCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(cancellationToken == null)
            {
                throw new TestCommandHandlerException("Cancellation token is null. Please check registration.");
            }

            BaseHandle(command);
            return Task.CompletedTask;
        }

        public async Task HandleAsync(DelayCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            await Task.Delay(command.DurationInMilliSeconds, cancellationToken);
            BaseHandle(command);
        }

        public Task HandleAsync(ThrowExceptionCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            BaseHandle(command);
            return Task.FromException(new TestCommandHandlerException("This is a triggered exception."));
        }

        protected virtual void BaseHandle<TCommand>(TCommand command) where TCommand : class
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            OutputHelper.WriteLine($"{DateTime.Now}: {GetType().Name} executed command of type {command.GetType().Name}.");
            InternalHandledCommands.Add(command);
        }
    }

    #endregion Command Handler

    #region Attributed Command Handlers

    public class TestAttributedCommandHandler : TestCommandHandler
    {

        public TestAttributedCommandHandler(ITestOutputHelper output)
            : base(output)
        {
        }

        [CommandHandler]
        public void HandleTestCommand(TestCommand command)
        {
            BaseHandle(command);
        }

        [CommandHandler]
        public void HandleThrowExceptionCommand(ThrowExceptionCommand command)
        {
            BaseHandle(command);
            throw new TestCommandHandlerException("This is a triggered exception.");
        }

        [CommandHandler]
        public Task HandleCancellableTestCommand(CancellableTestCommand command, CancellationToken ctx)
        {
            if(ctx == null)
            {
                return Task.FromException(new TestCommandHandlerException("Cancellation token is null. Please check attribute registration."));
            }

            BaseHandle(command);
            return Task.CompletedTask;
        }

        [CommandHandler]
        public async Task HandleDelayCommand(DelayCommand command, CancellationToken ctx)
        {
            await Task.Delay(command.DurationInMilliSeconds, ctx);

            BaseHandle(command);
        }
    }

    /// <summary>
    /// This will not be allowed.
    /// </summary>
    public class TestAttributedSyncCommandHandlerWithCancellationToken : TestCommandHandler
    {
        public TestAttributedSyncCommandHandlerWithCancellationToken(ITestOutputHelper output)
            : base(output)
        {
        }

        [CommandHandler]
        public void HandleTestCommand(TestCommand command, CancellationToken cancellationToken)
        {
            BaseHandle(command);
        }
    }

    #endregion Attributed Command Handlers
    
    public class TestCommandHandlerException : Exception
    {
        public TestCommandHandlerException() { }
        public TestCommandHandlerException(string message) : base(message) { }
    }
}
