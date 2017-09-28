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
        #region Register Method

        public class RegisterMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public RegisterMethod(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            [Fact]
            public void Async_Void_Handlers_Should_Not_Be_Allowed()
            {
                Assert.Throws<InvalidOperationException>(() =>
                {
                    try
                    {
                        var registration = new CommandHandlerAttributeRegistration();
                        registration.RegisterAttributedMethods(() => new TestAttributedCommandHandlerWithAsyncVoid(_outputHelper));
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }

        #endregion Register Method

        #region DispatchAsync Method

        public class DispatchAsyncMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public DispatchAsyncMethod(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            [Fact]
            public void Dispatch_Command_To_Attributed_Object()
            {
                var registration = new CommandHandlerAttributeRegistration();
                registration.RegisterAttributedMethods(() => new TestAttributedCommandHandler(_outputHelper));

                var dispatcher = new CommandDispatcher(registration);
                dispatcher.DispatchAsync(new DoSomethingAsyncCommand());
            }

            [Fact]
            public void Dispatch_Command_To_Attributed_Object_With_Cancellation()
            {
                var registration = new CommandHandlerAttributeRegistration();
                registration.RegisterAttributedMethods(() => new TestAttributedCommandHandler(_outputHelper));

                var cts = new CancellationTokenSource();

                var dispatcher = new CommandDispatcher(registration);
                dispatcher.DispatchAsync(new DoSomethingAsyncWithCancellationCommand(), cts.Token);
            }

            [Fact]
            public Task Cancel_Dispatch()
            {
                return Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
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

            [Fact]
            public Task Dispatch_Should_Propagate_Exceptions_From_Handlers()
            {
                return Assert.ThrowsAnyAsync<Exception>(async () =>
                {
                    try
                    {
                        var registration = new CommandHandlerAttributeRegistration();
                        registration.RegisterAttributedMethods(() => new TestAttributedCommandHandler(_outputHelper));

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
            public void Dispatch_Command_To_Attributed_Object()
            {
                var registration = new CommandHandlerAttributeRegistration();
                registration.RegisterAttributedMethods(() => new TestAttributedCommandHandler(_outputHelper));

                var dispatcher = new CommandDispatcher(registration);
                dispatcher.Dispatch(new DoSomethingCommand());
            }

            [Fact]
            public void Dispatch_Should_Propagate_Exceptions_From_Handlers()
            {
                Assert.ThrowsAny<Exception>(() =>
                {
                    try
                    {
                        var registration = new CommandHandlerAttributeRegistration();
                        registration.RegisterAttributedMethods(() => new TestAttributedCommandHandler(_outputHelper));

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
