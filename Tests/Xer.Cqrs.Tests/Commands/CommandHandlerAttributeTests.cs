using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack.Dispatchers;
using Xer.Cqrs.CommandStack.Registrations;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Commands
{
    public class CommandHandlerAttributeTests
    {
        #region DispatchAsync Method

        public class DispatchAsyncMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public DispatchAsyncMethod(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            [Fact]
            public void Should_Invoke_Registered_Attributed_Command_Handler()
            {
                var registration = new CommandHandlerAttributeRegistration();
                registration.Register(() => new TestAttributedCommandHandler(_outputHelper));

                var dispatcher = new CommandDispatcher(registration);
                dispatcher.DispatchAsync(new DoSomethingAsyncCommand());
            }

            [Fact]
            public void Should_Invoke_Registered_Attributed_Command_Handler_With_Cancellation()
            {
                var registration = new CommandHandlerAttributeRegistration();
                registration.Register(() => new TestAttributedCommandHandler(_outputHelper));

                var cts = new CancellationTokenSource();

                var dispatcher = new CommandDispatcher(registration);
                dispatcher.DispatchAsync(new DoSomethingAsyncWithCancellationCommand(), cts.Token);
            }

            [Fact]
            public Task Should_Throw_When_Cancelled()
            {
                return Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                {
                    var registration = new CommandHandlerAttributeRegistration();
                    registration.Register(() => new TestAttributedCommandHandler(_outputHelper));

                    var cts = new CancellationTokenSource();

                    var dispatcher = new CommandDispatcher(registration);
                    Task task = dispatcher.DispatchAsync(new DoSomethingAsyncForSpecifiedDurationCommand(1000), cts.Token);

                    cts.Cancel();

                    await task;
                });
            }

            [Fact]
            public Task Should_Propagate_Exceptions_From_Comand_Handler()
            {
                return Assert.ThrowsAnyAsync<Exception>(async () =>
                {
                    try
                    {
                        var registration = new CommandHandlerAttributeRegistration();
                        registration.Register(() => new TestAttributedCommandHandler(_outputHelper));

                        var dispatcher = new CommandDispatcher(registration);
                        await dispatcher.DispatchAsync(new ThrowExceptionCommand());
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }

        #endregion DispatchAsync Method

        #region Dispatch Method

        public class DispatchMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public DispatchMethod(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            [Fact]
            public void Should_Invoke_Registered_Attributed_Command_Handler()
            {
                var registration = new CommandHandlerAttributeRegistration();
                registration.Register(() => new TestAttributedCommandHandler(_outputHelper));

                var dispatcher = new CommandDispatcher(registration);
                dispatcher.Dispatch(new DoSomethingCommand());
            }

            [Fact]
            public void Should_Propagate_Exceptions_From_Command_Handler()
            {
                Assert.ThrowsAny<Exception>(() =>
                {
                    try
                    {
                        var registration = new CommandHandlerAttributeRegistration();
                        registration.Register(() => new TestAttributedCommandHandler(_outputHelper));

                        var dispatcher = new CommandDispatcher(registration);
                        dispatcher.Dispatch(new ThrowExceptionCommand());
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }

        #endregion Dispatch Method
    }
}
