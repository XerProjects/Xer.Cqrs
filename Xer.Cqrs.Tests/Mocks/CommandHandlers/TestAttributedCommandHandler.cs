using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.AttributeHandlers;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Mocks.CommandHandlers
{
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
}
