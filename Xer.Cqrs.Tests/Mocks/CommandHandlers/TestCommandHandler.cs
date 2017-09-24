using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Mocks.CommandHandlers
{
    public class TestCommandHandler : ICommandAsyncHandler<DoSomethingCommand>,
                                      ICommandAsyncHandler<DoSomethingAsyncCommand>,
                                      ICommandAsyncHandler<DoSomethingAsyncWithCancellationCommand>,
                                      ICommandAsyncHandler<DoSomethingAsyncForSpecifiedDurationCommand>,
                                      ICommandHandler<DoSomethingCommand>,
                                      ICommandHandler<DoSomethingAsyncCommand>,
                                      ICommandHandler<DoSomethingAsyncWithCancellationCommand>,
                                      ICommandHandler<DoSomethingAsyncForSpecifiedDurationCommand>
    {
        private readonly ITestOutputHelper _outputHelper;

        public TestCommandHandler(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

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

        private void handle<TCommand>(TCommand command) where TCommand : ICommand
        {
            _outputHelper.WriteLine($"Executed {command.CommandId} of type {command.GetType()} on {DateTime.Now}");
        }
    }
}
