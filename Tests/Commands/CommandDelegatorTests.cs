using SimpleInjector;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Cqrs.Tests.Mocks;
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
            public async Task Should_Invoke_Registered_Command_Handler()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);
                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandler(() => (ICommandAsyncHandler<DoSomethingCommand>)commandHandler);

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                var dispatcher = new MessageDelegator(resolver);
                await dispatcher.SendAsync(new DoSomethingCommand());

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is DoSomethingCommand);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Command_Handler_With_CancellationToken()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);
                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandler(() => (ICommandAsyncHandler<DoSomethingCommand>)commandHandler);

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                var dispatcher = new MessageDelegator(resolver);

                var cts = new CancellationTokenSource();
                await dispatcher.SendAsync(new DoSomethingCommand(), cts.Token);

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is DoSomethingCommand);
            }

            [Fact]
            public Task Should_Throw_When_No_Registered_Command_Handler_Is_Found()
            {
                return Assert.ThrowsAsync<NoMessageHandlerResolvedException>(async () =>
                {
                    var registration = new SingleMessageHandlerRegistration();
                    IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();
                    var dispatcher = new MessageDelegator(new RequiredMessageHandlerResolver(resolver));

                    try
                    {
                        await dispatcher.SendAsync(new DoSomethingCommand());
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
            public async Task Should_Invoke_Registered_Attributed_Command_Handler()
            {
                var commandHandler = new TestAttributedCommandHandler(_outputHelper);
                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandlerAttributes(() => commandHandler);

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();
                
                var dispatcher = new MessageDelegator(resolver);
                await dispatcher.SendAsync(new DoSomethingCommand());

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is DoSomethingCommand);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Attributed_Command_Handler_With_Cancellation()
            {
                var commandHandler = new TestAttributedCommandHandler(_outputHelper);
                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandlerAttributes(() => commandHandler);

                var cts = new CancellationTokenSource();

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                var dispatcher = new MessageDelegator(resolver);
                await dispatcher.SendAsync(new DoSomethingWithCancellationCommand(), cts.Token);

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is DoSomethingWithCancellationCommand);
            }

            [Fact]
            public Task Should_Throw_When_No_Registered_Attribute_Command_Handler_Is_Found()
            {
                return Assert.ThrowsAsync<NoMessageHandlerResolvedException>(async () =>
                {
                    var registration = new SingleMessageHandlerRegistration();
                    IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                    try
                    {
                        var dispatcher = new MessageDelegator(new RequiredMessageHandlerResolver(resolver));
                        await dispatcher.SendAsync(new DoSomethingCommand());
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
            public async Task Should_Invoke_Registered_Command_Handler_In_Container()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);
                var container = new Container();
                container.Register<ICommandHandler<DoSomethingCommand>>(() => commandHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var dispatcher = new MessageDelegator(new ContainerCommandHandlerResolver(containerAdapter)); // Sync handler resolver

                await dispatcher.SendAsync(new DoSomethingCommand());

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is DoSomethingCommand);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Command_Handler_In_Container_With_Cancellation()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);
                var container = new Container();
                container.Register<ICommandAsyncHandler<DoSomethingWithCancellationCommand>>(() => commandHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var dispatcher = new MessageDelegator(new ContainerCommandAsyncHandlerResolver(containerAdapter)); // Async handler resolver

                var cts = new CancellationTokenSource();

                await dispatcher.SendAsync(new DoSomethingWithCancellationCommand(), cts.Token);

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is DoSomethingWithCancellationCommand);
            }

            // TODO: Fix once CompositeMessageHandlerResolver is released.
            // [Fact]
            // public async Task Should_Invoke_Registered_Command_Handler_With_Composite_Resolver()
            // {
            //     var commandHandler = new TestCommandHandler(_outputHelper);
            //     var container = new Container();
            //     container.Register<ICommandHandler<DoSomethingCommand>>(() => commandHandler, Lifestyle.Singleton);
            //     container.Register<ICommandAsyncHandler<DoSomethingWithCancellationCommand>>(() => commandHandler, Lifestyle.Singleton);

            //     var containerAdapter = new SimpleInjectorContainerAdapter(container);
            //     var containerAsyncHandlerResolver = new ContainerCommandAsyncHandlerResolver(containerAdapter);
            //     var containerHandlerResolver = new ContainerCommandHandlerResolver(containerAdapter);

            //     Func<Exception, bool> exceptionHandler = (ex) => 
            //     {
            //         var exception = ex as NoCommandHandlerResolvedException;
            //         if (exception != null) 
            //         {
            //             _outputHelper.WriteLine($"Ignoring encountered exception while trying to resolve command handler for {exception.CommandType.Name}...");
                        
            //             // Notify as handled if no command handler is resolved from other resolvers.
            //             return true;
            //         }

            //         return false;
            //     };

            //     var compositeResolver = new CompositeMessageHandlerResolver(new List<ICommandHandlerResolver>()
            //     {
            //         containerAsyncHandlerResolver,
            //         containerHandlerResolver
            //     }, exceptionHandler); // Pass in an exception handler.

            //     var dispatcher = new MessageDispatcher(compositeResolver); // Composite resolver

            //     await dispatcher.DispatchAsync(new DoSomethingCommand());
            //     await dispatcher.DispatchAsync(new DoSomethingWithCancellationCommand());

            //     Assert.Equal(2, commandHandler.HandledCommands.Count);
            //     Assert.Contains(commandHandler.HandledCommands, c => c is DoSomethingCommand);
            //     Assert.Contains(commandHandler.HandledCommands, c => c is DoSomethingWithCancellationCommand);
            // }

            [Fact]
            public Task Should_Throw_When_No_Registered_Command_Handler_In_Container_Is_Found()
            {
                return Assert.ThrowsAsync<NoMessageHandlerResolvedException>(async () =>
                {
                    var container = new Container();
                    var containerAdapter = new SimpleInjectorContainerAdapter(container);
                    var dispatcher = new MessageDelegator(new ContainerCommandHandlerResolver(containerAdapter)); // Sync handler resolver

                    try
                    {
                        await dispatcher.SendAsync(new DoSomethingCommand());
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            #endregion Container Registration

            [Fact]
            public Task Should_Throw_When_Cancelled()
            {
                return Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                {
                    var commandHandler = new TestCommandHandler(_outputHelper);
                    var registration = new SingleMessageHandlerRegistration();
                    registration.RegisterCommandHandler(() => (ICommandAsyncHandler<DoSomethingForSpecifiedDurationCommand>)commandHandler);

                    var cts = new CancellationTokenSource();

                    IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                    var dispatcher = new MessageDelegator(resolver);
                    Task task = dispatcher.SendAsync(new DoSomethingForSpecifiedDurationCommand(2000), cts.Token);

                    cts.Cancel();

                    try
                    {
                        await task;
                    }
                    catch(Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            [Fact]
            public Task Should_Propagate_Exception_From_Command_Handler()
            {
                return Assert.ThrowsAnyAsync<Exception>(async () =>
                {
                    var commandHandler = new TestCommandHandler(_outputHelper);
                    var registration = new SingleMessageHandlerRegistration();
                    registration.RegisterCommandHandler(() => (ICommandAsyncHandler<ThrowExceptionCommand>)commandHandler);

                    IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                    var dispatcher = new MessageDelegator(resolver);

                    try
                    {
                        await dispatcher.SendAsync(new ThrowExceptionCommand());
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }

        #endregion SendAsync Method

        #region Send Method

        // TODO: Fix once MessageDispatcher supports synchronous Dispatch method.

        // public class DispatchMethod
        // {
        //     private readonly ITestOutputHelper _outputHelper;

        //     public DispatchMethod(ITestOutputHelper output)
        //     {
        //         _outputHelper = output;
        //     }
            
        //     #region Basic Registration

        //     [Fact]
        //     public void Should_Invoke_Registered_Command_Handler()
        //     {
        //         var commandHandler = new TestCommandHandler(_outputHelper);
        //         var registration = new CommandHandlerRegistration();
        //         registration.RegisterCommandHandler(() => (ICommandHandler<DoSomethingCommand>)commandHandler);

        //         var dispatcher = new CommandDispatcher(registration);
        //         dispatcher.Dispatch(new DoSomethingCommand());

        //         Assert.Equal(1, commandHandler.HandledCommands.Count);
        //         Assert.Contains(commandHandler.HandledCommands, c => c is DoSomethingCommand);
        //     }
            
        //     [Fact]
        //     public void Should_Throw_When_No_Registered_Command_Handler_Is_Found()
        //     {
        //         Assert.Throws<NoCommandHandlerResolvedException>(() =>
        //         {
        //             var registration = new CommandHandlerRegistration();
        //             var dispatcher = new CommandDispatcher(registration);

        //             try
        //             {
        //                 dispatcher.Dispatch(new DoSomethingCommand());
        //             }
        //             catch (Exception ex)
        //             {
        //                 _outputHelper.WriteLine(ex.ToString());
        //                 throw;
        //             }
        //         });
        //     }

        //     #endregion Basic Registration

        //     #region Attribute Registration

        //     [Fact]
        //     public void Should_Invoke_Registered_Attributed_Command_Handler()
        //     {
        //         var commandHandler = new TestAttributedCommandHandler(_outputHelper);
        //         var registration = new CommandHandlerAttributeMethodSelector();
        //         registration.Register(() => commandHandler);

        //         var dispatcher = new CommandDispatcher(registration);
        //         dispatcher.Dispatch(new DoSomethingCommand());

        //         Assert.Equal(1, commandHandler.HandledCommands.Count);
        //         Assert.Contains(commandHandler.HandledCommands, c => c is DoSomethingCommand);
        //     }

        //     [Fact]
        //     public void Should_Throw_When_No_Registered_Attribute_Command_Handler_Is_Found()
        //     {
        //         Assert.Throws<NoCommandHandlerResolvedException>(() =>
        //         {
        //             var registration = new CommandHandlerAttributeMethodSelector();
        //             var dispatcher = new CommandDispatcher(registration);

        //             try
        //             {
        //                 dispatcher.Dispatch(new DoSomethingCommand());
        //             }
        //             catch (Exception ex)
        //             {
        //                 _outputHelper.WriteLine(ex.ToString());
        //                 throw;
        //             }
        //         });
        //     }

        //     #endregion Attribute Registration

        //     #region Container Registration

        //     [Fact]
        //     public void Should_Invoke_Registered_Command_Handler_In_Container()
        //     {
        //         var commandHandler = new TestCommandHandler(_outputHelper);
        //         var container = new Container();
        //         container.Register<ICommandHandler<DoSomethingCommand>>(() => commandHandler, Lifestyle.Singleton);

        //         var containerAdapter = new SimpleInjectorContainerAdapter(container);

        //         var dispatcher = new CommandDispatcher(new ContainerCommandHandlerResolver(containerAdapter)); // Sync handler resolver
        //         dispatcher.Dispatch(new DoSomethingCommand());

        //         Assert.Equal(1, commandHandler.HandledCommands.Count);
        //         Assert.Contains(commandHandler.HandledCommands, c => c is DoSomethingCommand);
        //     }

        //     [Fact]
        //     public void Should_Throw_When_No_Registered_Command_Handler_In_Container_Is_Found()
        //     {
        //         Assert.Throws<NoCommandHandlerResolvedException>(() =>
        //         {
        //             var container = new Container();
        //             var containerAdapter = new SimpleInjectorContainerAdapter(container);
        //             var dispatcher = new CommandDispatcher(new ContainerCommandAsyncHandlerResolver(containerAdapter)); // Async handler resolver

        //             try
        //             {
        //                 dispatcher.Dispatch(new DoSomethingCommand());
        //             }
        //             catch (Exception ex)
        //             {
        //                 _outputHelper.WriteLine(ex.ToString());
        //                 throw;
        //             }
        //         });
        //     }

        //     #endregion Container Registration

        //     [Fact]
        //     public void Should_Propagate_Exception_From_Command_Handler()
        //     {
        //         Assert.ThrowsAny<Exception>(() =>
        //         {
        //             var registration = new CommandHandlerRegistration();
        //             registration.RegisterCommandHandler(() => (ICommandHandler<ThrowExceptionCommand>)new TestCommandHandler(_outputHelper));

        //             var dispatcher = new CommandDispatcher(registration);

        //             try
        //             {
        //                 dispatcher.Dispatch(new ThrowExceptionCommand());
        //             }
        //             catch (Exception ex)
        //             {
        //                 _outputHelper.WriteLine(ex.ToString());
        //                 throw;
        //             }
        //         });
        //     }
        // }

        #endregion Send Method
    }
}
