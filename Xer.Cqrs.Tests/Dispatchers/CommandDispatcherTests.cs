using SimpleInjector;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Dispatchers;
using Xer.Cqrs.CommandStack.Registrations;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Cqrs.Tests.Mocks;
using Xer.Cqrs.Tests.Mocks.CommandHandlers;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests
{
    public class CommandDispatcherTests
    {
        #region DispatchAsync Method

        public class DispatchAsyncMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public DispatchAsyncMethod(ITestOutputHelper output)
            {
                _outputHelper = output;
            }

            [Fact]
            public async Task Dispatch_Command_To_Registered_Command_Handler_In_Container()
            {
                var container = new Container();
                container.Register<ICommandAsyncHandler<DoSomethingAsyncCommand>>(() => new TestCommandHandler(_outputHelper));

                var containerAdapter = new SimpleInjectorContainerAdapter(container);

                var dispatcher = new CommandDispatcher(new ContainerCommandHandlerResolver(containerAdapter));
                await dispatcher.DispatchAsync(new DoSomethingAsyncCommand());
            }
            
            [Fact]
            public async Task Dispatch_Command_To_Registered_Command_Handler()
            {
                var registration = new CommandHandlerRegistration();
                registration.Register(() => (ICommandAsyncHandler<DoSomethingAsyncCommand>)new TestCommandHandler(_outputHelper));

                var dispatcher = new CommandDispatcher(registration);
                await dispatcher.DispatchAsync(new DoSomethingAsyncCommand());
            }

            [Fact]
            public async Task Dispatch_Command_To_Registered_Command_Handler_With_CancellationToken()
            {
                var registration = new CommandHandlerRegistration();
                registration.Register(() => (ICommandAsyncHandler<DoSomethingAsyncCommand>)new TestCommandHandler(_outputHelper));

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
                    registration.RegisterCommandHandlerAttributes(() => new TestAttributedCommandHandler(_outputHelper));

                    var cts = new CancellationTokenSource();

                    var dispatcher = new CommandDispatcher(registration);
                    Task task = dispatcher.DispatchAsync(new DoSomethingAsyncForSpecifiedDurationCommand(1000), cts.Token);

                    cts.Cancel();

                    await task;
                });
            }

            [Fact]
            public Task DispatchDispatcher_Should_Propagate_Exceptions_From_Handlers()
            {
                return Assert.ThrowsAnyAsync<Exception>(() =>
                {
                    try
                    {
                        var registration = new CommandHandlerRegistration();
                        registration.Register(() => (ICommandAsyncHandler<ThrowExceptionCommand>)new TestCommandHandler(_outputHelper));

                        var dispatcher = new CommandDispatcher(registration);
                        return dispatcher.DispatchAsync(new ThrowExceptionCommand());
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

            public DispatchMethod(ITestOutputHelper output)
            {
                _outputHelper = output;
            }

            [Fact]
            public void Dispatch_Command_To_Registered_Command_Handler()
            {
                var registration = new CommandHandlerRegistration();
                registration.Register(() => (ICommandHandler<DoSomethingAsyncCommand>)new TestCommandHandler(_outputHelper));

                var dispatcher = new CommandDispatcher(registration);
                dispatcher.Dispatch(new DoSomethingAsyncCommand());
            }

            [Fact]
            public void Dispatch_Should_Propagate_Exceptions_From_Handlers()
            {
                Assert.ThrowsAny<Exception>(() =>
                {
                    try
                    {
                        var registration = new CommandHandlerRegistration();
                        registration.Register(() => (ICommandHandler<ThrowExceptionCommand>)new TestCommandHandler(_outputHelper));

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
