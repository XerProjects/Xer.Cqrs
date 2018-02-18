using SimpleInjector;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Cqrs.Tests.Entities;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using Xer.Delegator.Registrations;
using Xer.Delegator;
using Xer.Delegator.Resolvers;
using Xer.Delegator.Exceptions;

namespace Xer.Cqrs.Tests.Commands
{
    public class CommandDelegatorTests
    {
        #region SendAsync Method

        public class SendAsyncMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public SendAsyncMethod(ITestOutputHelper output)
            {
                _outputHelper = output;
            }

            #region Basic Registration

            [Fact]
            public async Task Should_Send_Command_To_Registered_Command_Handler()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);
                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandler(() => (ICommandAsyncHandler<TestCommand>)commandHandler);

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                var delegator = new CommandDelegator(resolver);
                await delegator.SendAsync(new TestCommand());

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is TestCommand);
            }

            [Fact]
            public async Task Should_Send_Command_To_Registered_Command_Handler_With_CancellationToken()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);
                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandler(() => (ICommandAsyncHandler<TestCommand>)commandHandler);

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                var delegator = new CommandDelegator(resolver);

                var cts = new CancellationTokenSource();
                await delegator.SendAsync(new TestCommand(), cts.Token);

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is TestCommand);
            }

            [Fact]
            public Task Should_Throw_If_No_Registered_Command_Handler_Is_Found()
            {
                return Assert.ThrowsAsync<NoMessageHandlerResolvedException>(async () =>
                {
                    var registration = new SingleMessageHandlerRegistration();
                    IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();
                    var delegator = new CommandDelegator(resolver);

                    try
                    {
                        await delegator.SendAsync(new TestCommand());
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            #endregion Basic Registration

            #region Attribute Registration

            [Fact]
            public async Task Should_Send_Command_To_Registered_Attributed_Command_Handler()
            {
                var commandHandler = new TestAttributedCommandHandler(_outputHelper);
                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandlerAttributes(() => commandHandler);

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();
                
                var delegator = new CommandDelegator(resolver);
                await delegator.SendAsync(new TestCommand());

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is TestCommand);
            }

            [Fact]
            public async Task Should_Send_Command_To_Registered_Attributed_Command_Handler_With_Cancellation()
            {
                var commandHandler = new TestAttributedCommandHandler(_outputHelper);
                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandlerAttributes(() => commandHandler);

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                var delegator = new CommandDelegator(resolver);
                using (var cts = new CancellationTokenSource())
                {
                    await delegator.SendAsync(new CancellableTestCommand(), cts.Token);
                }

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is CancellableTestCommand);
            }

            [Fact]
            public Task Should_Throw_If_Registration_Instance_Factory_Produces_Null()
            {
                return Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    var commandHandler = new TestAttributedCommandHandler(_outputHelper);

                    var registration = new SingleMessageHandlerRegistration();
                    registration.RegisterCommandHandlerAttributes(CommandHandlerAttributeMethod.FromType<TestAttributedCommandHandler>(() => null));

                    try
                    {
                        var delegator = new CommandDelegator(registration.BuildMessageHandlerResolver());
                        await delegator.SendAsync(new TestCommand());
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            [Fact]
            public Task Should_Throw_If_Instance_From_Factory_Does_Not_Match_Registration_Type()
            {
                return Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    var commandHandler = new TestAttributedCommandHandler(_outputHelper);

                    var registration = new SingleMessageHandlerRegistration();
                    registration.RegisterCommandHandlerAttributes(CommandHandlerAttributeMethod.FromType(typeof(TestAttributedCommandHandler), 
                                                                                                         () => new TestEventHandler(_outputHelper)));

                    try
                    {
                        var delegator = new CommandDelegator(registration.BuildMessageHandlerResolver());
                        await delegator.SendAsync(new TestCommand());
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            [Fact]
            public Task Should_Propagate_If_Registration_Instance_Factory_Throws_An_Exception()
            {
                return Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    var commandHandler = new TestAttributedCommandHandler(_outputHelper);

                    var registration = new SingleMessageHandlerRegistration();
                    registration.RegisterCommandHandlerAttributes(CommandHandlerAttributeMethod.FromType<TestAttributedCommandHandler>(() => throw new Exception("Intentional exception.")));

                    try
                    {
                        var delegator = new CommandDelegator(registration.BuildMessageHandlerResolver());
                        await delegator.SendAsync(new TestCommand());
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            #endregion Attribute Registration

            #region Container Registration

            [Fact]
            public async Task Should_Send_Command_To_Registered_Command_Handler_In_Container()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);
                var container = new Container();
                container.RegisterSingleton<ICommandHandler<TestCommand>>(() => commandHandler);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var delegator = new CommandDelegator(new ContainerCommandHandlerResolver(containerAdapter)); // Sync handler resolver

                await delegator.SendAsync(new TestCommand());

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is TestCommand);
            }

            [Fact]
            public async Task Should_Send_Command_To_Registered_Command_Handler_In_Container_With_Cancellation()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);
                var container = new Container();
                container.RegisterSingleton<ICommandAsyncHandler<CancellableTestCommand>>(() => commandHandler);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var delegator = new CommandDelegator(new ContainerCommandAsyncHandlerResolver(containerAdapter)); // Async handler resolver

                using(var cts = new CancellationTokenSource())
                {
                    await delegator.SendAsync(new CancellableTestCommand(), cts.Token);

                    Assert.Equal(1, commandHandler.HandledCommands.Count);
                    Assert.Contains(commandHandler.HandledCommands, c => c is CancellableTestCommand);
                }
            }

            [Fact]
            public async Task Should_Send_Command_To_Registered_Command_Handler_In_Composite_Resolver()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);
                var container = new Container();
                container.RegisterSingleton<ICommandHandler<TestCommand>>(() => commandHandler);
                container.RegisterSingleton<ICommandAsyncHandler<CancellableTestCommand>>(() => commandHandler);

                // Exception handler will log and ignore exception.
                Func<Exception, bool> exceptionHandler = (ex) => 
                {
                    if (ex != null) 
                    {
                        _outputHelper.WriteLine($"Ignoring encountered exception while trying to resolve command handler: {ex.Message}");
                        
                        // Notify as handled if no command handler is resolved from other resolvers.
                        return true;
                    }

                    return false;
                };

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var containerAsyncHandlerResolver = new ContainerCommandAsyncHandlerResolver(containerAdapter, exceptionHandler);
                var containerHandlerResolver = new ContainerCommandHandlerResolver(containerAdapter, exceptionHandler);

                CompositeMessageHandlerResolver compositeResolver = CompositeMessageHandlerResolver.Compose(
                    containerAsyncHandlerResolver,
                    containerHandlerResolver);

                var delegator = new CommandDelegator(compositeResolver); // Composite resolver

                await delegator.SendAsync(new TestCommand());
                await delegator.SendAsync(new CancellableTestCommand());

                Assert.Equal(2, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is TestCommand);
                Assert.Contains(commandHandler.HandledCommands, c => c is CancellableTestCommand);
            }

            [Fact]
            public Task Should_Throw_When_No_Registered_Command_Handler_In_Container_Is_Found()
            {
                return Assert.ThrowsAsync<NoMessageHandlerResolvedException>(async () =>
                {
                    var container = new Container();
                    var containerAdapter = new SimpleInjectorContainerAdapter(container);
                    var delegator = new CommandDelegator(new ContainerCommandHandlerResolver(containerAdapter)); // Sync handler resolver

                    try
                    {
                        await delegator.SendAsync(new TestCommand());
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            #endregion Container Registration
        }

        #endregion SendAsync Method
    }
}
