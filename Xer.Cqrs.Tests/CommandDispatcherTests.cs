using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.AttributeHandlers.Registrations;
using Xer.Cqrs.Dispatchers;
using Xer.Cqrs.Registrations.CommandHandlers;
using Xer.Cqrs.Tests.Mocks;
using Xer.Cqrs.Tests.Mocks.CommandHandlers;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests
{
    public class CommandDispatcherTests
    {
        public class DispatchAsyncMethod
        {
            private readonly ITestOutputHelper _output;

            public DispatchAsyncMethod(ITestOutputHelper output)
            {
                _output = output;
            }

            [Fact]
            public async Task Dispatch_Command_To_Registered_Command_Handler()
            {
                var registration = new CommandHandlerRegistration();
                registration.Register(() => (ICommandAsyncHandler<DoSomethingAsyncCommand>)new TestCommandHandler(_output));

                var dispatcher = new CommandDispatcher(registration);
                await dispatcher.DispatchAsync(new DoSomethingAsyncCommand());
            }

            [Fact]
            public async Task Dispatch_Command_To_Registered_Command_Handler_With_CancellationToken()
            {
                var registration = new CommandHandlerRegistration();
                registration.Register(() => (ICommandAsyncHandler<DoSomethingAsyncCommand>)new TestCommandHandler(_output));

                var cts = new CancellationTokenSource();

                var dispatcher = new CommandDispatcher(registration);
                await dispatcher.DispatchAsync(new DoSomethingAsyncCommand(), cts.Token);
            }

            [Fact]
            public void Cancel_Dispatch()
            {
                Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                {
                    var registration = new CommandHandlerAttributeRegistration();
                    registration.RegisterAttributedMethods(() => new TestAttributedCommandHandler(_output));

                    var cts = new CancellationTokenSource();

                    var dispatcher = new CommandDispatcher(registration);
                    Task task = dispatcher.DispatchAsync(new DoSomethingAsyncForSpecifiedDurationCommand(1000), cts.Token);

                    cts.Cancel();

                    await task;
                });
            }
        }

        public class DispatchMethod
        {
            private readonly ITestOutputHelper _output;

            public DispatchMethod(ITestOutputHelper output)
            {
                _output = output;
            }

            [Fact]
            public void Dispatch_Command_To_Registered_Command_Handler()
            {
                var registration = new CommandHandlerRegistration();
                registration.Register(() => (ICommandAsyncHandler<DoSomethingAsyncCommand>)new TestCommandHandler(_output));

                var dispatcher = new CommandDispatcher(registration);
                dispatcher.Dispatch(new DoSomethingAsyncCommand());
            }
        }
    }
}
