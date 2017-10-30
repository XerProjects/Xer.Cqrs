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
    #region Command Handlers

    public class TestCommandHandler : ICommandAsyncHandler<DoSomethingCommand>,
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
        private readonly ITestOutputHelper _outputHelper;
        private List<ICommand> _handledCommands = new List<ICommand>();

        public TestCommandHandler(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public IReadOnlyCollection<ICommand> HandledCommands => _handledCommands.AsReadOnly();

        public void Handle(DoSomethingCommand command)
        {
            handle(command);
        }

        public void Handle(DoSomethingAsyncCommand command)
        {
            handle(command);
        }

        public void Handle(DoSomethingAsyncWithCancellationCommand command)
        {
            handle(command);
        }

        public void Handle(DoSomethingAsyncForSpecifiedDurationCommand command)
        {
            handle(command);
        }

        public void Handle(ThrowExceptionCommand command)
        {
            handle(command);

            throw new NotImplementedException("This will fail.");
        }

        public Task HandleAsync(DoSomethingCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(command);

            return Task.CompletedTask;
        }

        public Task HandleAsync(DoSomethingAsyncCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(command);

            return Task.CompletedTask;
        }

        public Task HandleAsync(DoSomethingAsyncWithCancellationCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(command);

            return Task.CompletedTask;
        }

        public Task HandleAsync(DoSomethingAsyncForSpecifiedDurationCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(command);

            return Task.CompletedTask;
        }

        public Task HandleAsync(ThrowExceptionCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(command);

            throw new NotImplementedException("This will fail.");
        }

        private void handle<TCommand>(TCommand command) where TCommand : ICommand
        {
            _outputHelper.WriteLine($"{GetType().Name} executed command of type {command.GetType().Name} on {DateTime.Now}.");
            _handledCommands.Add(command);
        }
    }

    #endregion Command Handlers

    #region Attributed Command Handlers

    public class TestAttributedCommandHandler
    {
        private readonly ITestOutputHelper _output;

        public TestAttributedCommandHandler(ITestOutputHelper output)
        {
            _output = output;
        }

        [CommandHandler]
        public void DoSomething(DoSomethingCommand command)
        {
            _output.WriteLine($"Handled {command.GetType().Name}~");
        }

        [CommandHandler]
        public void DoSomethingWithException(ThrowExceptionCommand command)
        {
            _output.WriteLine($"Handled {command.GetType().Name}~");

            throw new NotImplementedException("This will fail.");
        }

        [CommandHandler]
        public Task DoSomethingAsync(DoSomethingAsyncCommand command)
        {
            _output.WriteLine($"Handled {command.GetType().Name}~");

            return Task.CompletedTask;
        }

        [CommandHandler]
        public Task DoSomethingAsync(DoSomethingAsyncWithCancellationCommand command, CancellationToken ctx)
        {
            _output.WriteLine($"Handled {command.GetType().Name}~");

            return Task.CompletedTask;
        }

        [CommandHandler]
        public async Task DoSomethingAsync(DoSomethingAsyncForSpecifiedDurationCommand command, CancellationToken ctx)
        {
            await Task.Delay(command.DurationInMilliSeconds, ctx);

            ctx.ThrowIfCancellationRequested();

            _output.WriteLine($"Handled {command.GetType().Name}~");
        }
    }

    public class TestAttributedCommandHandlerWithAsyncVoid
    {
        private readonly ITestOutputHelper _output;

        public TestAttributedCommandHandlerWithAsyncVoid(ITestOutputHelper output)
        {
            _output = output;
        }

        [CommandHandler]
        public async void DoAsyncVoidHandlerCommand(DoAsyncVoidHandlerCommand command)
        {
            _output.WriteLine($"Handled {command.GetType().Name}~");

            await Task.Yield();
        }
    }

    public class TestAttributedSyncCommandHandlerWithCancellationToken
    {
        private readonly ITestOutputHelper _output;

        public TestAttributedSyncCommandHandlerWithCancellationToken(ITestOutputHelper output)
        {
            _output = output;
        }

        [CommandHandler]
        public void Handle(DoSomethingCommand command, CancellationToken cancellationToken)
        {
            _output.WriteLine($"Handled {command.GetType().Name}~");
        }
    }

    #endregion Attributed Command Handlers
}
