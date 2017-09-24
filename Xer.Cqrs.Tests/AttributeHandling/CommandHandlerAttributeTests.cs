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

namespace Xer.Cqrs.Tests.AttributeHandling
{
    public class CommandHandlerAttributeTests
    {
        public class Registration
        {
            private readonly ITestOutputHelper _outputHelper;

            public Registration(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            [Fact]
            public void RegisterAttributedMethods_Must_Register_Methods_Marked_With_CommandHandler_Attribute()
            {
                var registration = new CommandHandlerAttributeRegistration();
                registration.RegisterAttributedMethods(() => new TestAttributedCommandHandler(_outputHelper));

                var dispatcher = new CommandDispatcher(new CompositeCommandHandlerProvider(new[] { registration }));
                dispatcher.DispatchAsync(new DoSomethingCommand());
            }

            [Fact]
            public void DispatchAsync_Command_To_Attributed_Object()
            {
                var registration = new CommandHandlerAttributeRegistration();
                registration.RegisterAttributedMethods(() => new TestAttributedCommandHandler(_outputHelper));

                var dispatcher = new CommandDispatcher(registration);
                dispatcher.DispatchAsync(new DoSomethingAsyncCommand());
            }

            [Fact]
            public void DispatchAsync_Command_To_Attributed_Object_With_Cancellation()
            {
                var registration = new CommandHandlerAttributeRegistration();
                registration.RegisterAttributedMethods(() => new TestAttributedCommandHandler(_outputHelper));

                var cts = new CancellationTokenSource();

                var dispatcher = new CommandDispatcher(registration);
                dispatcher.DispatchAsync(new DoSomethingAsyncWithCancellationCommand(), cts.Token);
            }

            [Fact]
            public void Cancel_Dispatch()
            {
                Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                {
                    var registration = new CommandHandlerAttributeRegistration();
                    registration.RegisterAttributedMethods(() => new TestAttributedCommandHandler(_outputHelper));

                    var cts = new CancellationTokenSource();

                    var dispatcher = new CommandDispatcher(registration);
                    Task task = dispatcher.DispatchAsync(new DoSomethingAsyncForSpecifiedDurationCommand(1000), cts.Token);

                    cts.Cancel();

                    await task;
                });
            }
        }
    }
}
