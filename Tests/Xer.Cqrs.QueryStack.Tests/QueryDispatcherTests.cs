using SimpleInjector;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Dispatchers;
using Xer.Cqrs.QueryStack.Registrations;
using Xer.Cqrs.QueryStack.Resolvers;
using Xer.Cqrs.QueryStack.Tests.Entities;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using FluentAssertions;

namespace Xer.Cqrs.QueryStack.Tests.Queries
{
    public class QueryDispatcherTests
    {
        #region DispatchAsync Method

        public class DispatchAsyncMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public DispatchAsyncMethod(ITestOutputHelper testOutputHelper)
            {
                _outputHelper = testOutputHelper;
            }

            #region Basic Registration

            [Fact]
            public async Task ShouldInvokeRegisteredQueryHandler()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomething, string>)new TestQueryHandler(_outputHelper));

                const string data = nameof(ShouldInvokeRegisteredQueryHandler);

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data));

                result.Should().Be(data);
            }

            [Fact]
            public async Task ShouldInvokeRegisteredQueryHandlerWhenDispatchedMultipleTimes()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomething, string>)queryHandler);
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingWithNonReferenceTypeResult, int>)new TestQueryHandler(_outputHelper));

                const string data1 = "Test message 1.";
                const string data2 = "Test message 2.";
                const int data3 = 1;

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.DispatchAsync<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(data3));

                await Task.WhenAll(result1, result2, result3);
                
                (await result1).Should().Be(data1);
                (await result2).Should().Be(data2);
                (await result3).Should().Be(data3);
            }

            [Fact]
            public async Task ShouldInvokeRegisteredQueryHandlerWithCancellationToken()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingWithDelay, string>)queryHandler);

                var cts = new CancellationTokenSource();

                var dispatcher = new QueryDispatcher(registration);

                const string data = nameof(ShouldInvokeRegisteredQueryHandlerWithCancellationToken);

                var result = await dispatcher.DispatchAsync<QuerySomethingWithDelay, string>(new QuerySomethingWithDelay(data, 500), cts.Token);

                result.Should().Be(data);
            }

            [Fact]
            public async Task ShouldAllowRegisteredQueryHandlersWithNonReferenceTypeQueryResults()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomethingWithNonReferenceTypeResult, int>)queryHandler);

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(1973));
                
                result.Should().Be(1973);
            }

            [Fact]
            public void ShouldThrowWhenNoRegisteredQueryHandlerIsFound()
            {
                var registration = new QueryHandlerRegistration();
                var dispatcher = new LoggingQueryDispatcher(new QueryDispatcher(registration), _outputHelper);

                const string data = nameof(ShouldThrowWhenNoRegisteredQueryHandlerIsFound);

                Func<Task> action = () =>
                {
                    return dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data));
                };

                action.Should().Throw<NoQueryHandlerResolvedException>();
            }

            #endregion Basic Registration

            #region Attribute Registration

            [Fact]
            public async Task ShouldInvokeRegisteredAttributeQueryHandler()
            {
                var queryHandler = new TestAttributedQueryHandler(_outputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);

                const string data = nameof(ShouldInvokeRegisteredAttributeQueryHandler);

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data));
                
                result.Should().Be(data);
            }

            [Fact]
            public async Task ShouldInvokeRegisteredAttributeQueryHandlerWhenDispatchedMultipleTimes()
            {
                var queryHandler = new TestAttributedQueryHandler(_outputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);

                const string data1 = "Test message 1.";
                const string data2 = "Test message 2.";
                const int data3 = 1;

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.DispatchAsync<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(data3));

                await Task.WhenAll(result1, result2, result3);

                (await result1).Should().Be(data1);
                (await result2).Should().Be(data2);
                (await result3).Should().Be(data3);
            }

            [Fact]
            public async Task ShouldInvokeRegisteredAttributeQueryHandlerWithCancellationToken()
            {
                var queryHandler = new TestAttributedQueryHandler(_outputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);
                var dispatcher = new QueryDispatcher(registration);

                var cts = new CancellationTokenSource();
                const string data = nameof(ShouldInvokeRegisteredAttributeQueryHandlerWithCancellationToken);
                
                var result = await dispatcher.DispatchAsync<QuerySomethingWithDelay, string>(new QuerySomethingWithDelay(data, 500), cts.Token);

                result.Should().Be(data);
            }

            [Fact]
            public async Task ShouldAllowAttributeQueryHandlersWithNonReferenceTypeQueryResults()
            {
                var queryHandler = new TestAttributedQueryHandler(_outputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(1973));

                result.Should().Be(1973);
            }

            [Fact]
            public void ShouldThrowWhenNoRegisteredAttributeQueryHandlerIsFound()
            {
                var registration = new QueryHandlerAttributeRegistration();
                var dispatcher = new LoggingQueryDispatcher(new QueryDispatcher(registration), _outputHelper);

                const string data = nameof(ShouldThrowWhenNoRegisteredAttributeQueryHandlerIsFound);

                Func<Task> action = () =>
                {
                    return dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data));
                };

                action.Should().Throw<NoQueryHandlerResolvedException>();
            }

            #endregion Attribute Registration

            #region Container Registration

            [Fact]
            public async Task ShouldInvokeRegisteredQueryHandlerInContainer()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomething, string>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryAsyncHandlerResolver(containerAdapter);

                const string data = nameof(ShouldInvokeRegisteredQueryHandlerInContainer);

                var dispatcher = new QueryDispatcher(resolver);
                var result = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data));

                result.Should().Be(data);
            }

            [Fact]
            public async Task ShouldInvokeRegisteredQueryHandlerInContainerWhenDispatchedMultipleTimes()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomething, string>>(() => queryHandler, Lifestyle.Singleton);
                container.Register<IQueryAsyncHandler<QuerySomethingWithNonReferenceTypeResult, int>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryAsyncHandlerResolver(containerAdapter);

                const string data1 = "Test message 1.";
                const string data2 = "Test message 2.";
                const int data3 = 1;

                var dispatcher = new QueryDispatcher(resolver);
                var result1 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.DispatchAsync<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(data3));

                await Task.WhenAll(result1, result2, result3);

                (await result1).Should().Be(data1);
                (await result2).Should().Be(data2);
                (await result3).Should().Be(data3);
            }

            [Fact]
            public async Task ShouldInvokeRegisteredQueryHandlerInContainerWithCancellationToken()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomethingWithDelay, string>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryAsyncHandlerResolver(containerAdapter);
                var dispatcher = new QueryDispatcher(resolver);

                var cts = new CancellationTokenSource();
                const string data = nameof(ShouldInvokeRegisteredAttributeQueryHandlerWithCancellationToken);

                var result = await dispatcher.DispatchAsync<QuerySomethingWithDelay, string>(new QuerySomethingWithDelay(data, 500), cts.Token);

                result.Should().Be(data);
            }

            [Fact]
            public async Task ShouldAllowRegisteredQueryHandlersInContainerWithNonReferenceTypeQueryResults()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomethingWithNonReferenceTypeResult, int>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryAsyncHandlerResolver(containerAdapter);
                var dispatcher = new QueryDispatcher(resolver);

                var result = await dispatcher.DispatchAsync<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(1973));

                result.Should().Be(1973);
            }

            [Fact]
            public void ShouldThrowWhenNoRegisteredQueryHandlerInContainerIsFound()
            {
                var container = new Container();
                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryHandlerResolver(containerAdapter);
                var dispatcher = new LoggingQueryDispatcher(new QueryDispatcher(resolver), _outputHelper);

                const string data = nameof(ShouldThrowWhenNoRegisteredAttributeQueryHandlerIsFound);
                
                Func<Task> action = () =>
                {
                    return dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data));
                };

                action.Should().Throw<NoQueryHandlerResolvedException>();
            }

            #endregion Container Registration

            [Fact]
            public void ShouldPropagateExceptionFromQueryHandler()
            {
                
                var queryHandler = new TestQueryHandler(_outputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingWithException, string>)queryHandler);
                var dispatcher = new LoggingQueryDispatcher(new QueryDispatcher(registration), _outputHelper);

                Func<Task> action = () =>
                {
                    return dispatcher.DispatchAsync<QuerySomethingWithException, string>(
                        new QuerySomethingWithException("This will cause an exception."));
                };

                action.Should().Throw<TestQueryHandlerException>();
            }

            [Fact]
            public void ShouldThrowWhenCancelled()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingWithDelay, string>)queryHandler);
                var dispatcher = new QueryDispatcher(registration);

                using(var cts = new CancellationTokenSource())
                {
                    Task task = dispatcher.DispatchAsync<QuerySomethingWithDelay, string>(
                        new QuerySomethingWithDelay("This will be cancelled", 3000), cts.Token);

                    // Cancel.
                    cts.Cancel();

                    Func<Task> cancelledTask = () => task;

                    cancelledTask.Should().Throw<OperationCanceledException>();
                }
            }
        }

        #endregion DispatchAsync Method

        #region Dispatch Method

        public class DispatchMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public DispatchMethod(ITestOutputHelper testOutputHelper)
            {
                _outputHelper = testOutputHelper;
            }

            #region Basic Registration

            [Fact]
            public void ShouldInvokeRegisteredQueryHandler()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomething, string>)queryHandler);

                const string data = nameof(ShouldInvokeRegisteredQueryHandler);

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));

                result.Should().Be(data);
            }

            [Fact]
            public void ShouldInvokeRegisteredQueryHandlerWhenDispatchedMultipleTimes()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomething, string>)queryHandler);
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingWithNonReferenceTypeResult, int>)new TestQueryHandler(_outputHelper));

                const string data1 = "Test message 1.";
                const string data2 = "Test message 2.";
                const int data3 = 1;

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.Dispatch<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(data3));

                result1.Should().Be(data1);
                result2.Should().Be(data2);
                result3.Should().Be(data3);
            }
            
            [Fact]
            public void ShouldAllowRegisteredQueryHandlersWithNonReferenceTypeQueryResults()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomethingWithNonReferenceTypeResult, int>)queryHandler);

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(1973));

                result.Should().Be(1973);
            }

            [Fact]
            public void ShouldThrowWhenNoRegisteredQueryHandlerIsFound()
            {
                var registration = new QueryHandlerRegistration();
                var dispatcher = new LoggingQueryDispatcher(new QueryDispatcher(registration), _outputHelper);
                const string data = nameof(ShouldThrowWhenNoRegisteredQueryHandlerIsFound);

                Action action = () => dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));

                action.Should().Throw<NoQueryHandlerResolvedException>();
            }

            #endregion Basic Registration

            #region Attribute Registration

            [Fact]
            public void ShouldInvokeRegisteredAttributeQueryHandler()
            {
                var queryHandler = new TestAttributedQueryHandler(_outputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);

                const string data = nameof(ShouldInvokeRegisteredAttributeQueryHandler);

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));

                result.Should().Be(data);
            }

            [Fact]
            public void ShouldInvokeRegisteredAttributeQueryHandlerWhenDispatchedMultipleTimes()
            {
                var queryHandler = new TestAttributedQueryHandler(_outputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);

                const string data1 = "Test message 1.";
                const string data2 = "Test message 2.";
                const int data3 = 1;

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.Dispatch<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(data3));
                                
                result1.Should().Be(data1);
                result2.Should().Be(data2);
                result3.Should().Be(data3);
            }

            [Fact]
            public void ShouldAllowAttributeQueryHandlersWithNonReferenceTypeQueryResults()
            {
                var queryHandler = new TestAttributedQueryHandler(_outputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(1973));

                result.Should().Be(1973);
            }

            [Fact]
            public void ShouldThrowWhenNoRegisteredAttributeQueryHandlerIsFound()
            {
                var registration = new QueryHandlerAttributeRegistration();
                var dispatcher = new LoggingQueryDispatcher(new QueryDispatcher(registration), _outputHelper);
                const string data = nameof(ShouldThrowWhenNoRegisteredAttributeQueryHandlerIsFound);

                Action action = () => dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));
                
                action.Should().Throw<NoQueryHandlerResolvedException>();
            }

            #endregion Attribute Registration

            #region Container Registration

            [Fact]
            public void ShouldInvokeRegisteredQueryHandlerInContainer()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var container = new Container();
                container.Register<IQueryHandler<QuerySomething, string>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryHandlerResolver(containerAdapter); // Sync handler resolver

                const string data = nameof(ShouldInvokeRegisteredQueryHandlerInContainer);

                var dispatcher = new QueryDispatcher(resolver);
                var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));

                result.Should().Be(data);
            }

            [Fact]
            public void ShouldInvokeRegisteredQueryHandlerInContainerWhenDispatchedMultipleTimes()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomething, string>>(() => queryHandler, Lifestyle.Singleton);
                container.Register<IQueryAsyncHandler<QuerySomethingWithNonReferenceTypeResult, int>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryAsyncHandlerResolver(containerAdapter); // Async handler resolver

                const string data1 = "Test message 1.";
                const string data2 = "Test message 2.";
                const int data3 = 1;

                var dispatcher = new QueryDispatcher(resolver);
                var result1 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.Dispatch<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(data3));
                
                result1.Should().Be(data1);
                result2.Should().Be(data2);
                result3.Should().Be(data3);
            }

            [Fact]
            public void ShouldAllowRegisteredQueryHandlersInContainerWithNonReferenceTypeQueryResults()
            {
                var queryHandler = new TestQueryHandler(_outputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomethingWithNonReferenceTypeResult, int>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryAsyncHandlerResolver(containerAdapter); // Async handler resolver
                var dispatcher = new QueryDispatcher(resolver);

                var result = dispatcher.Dispatch<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(1973));

                result.Should().Be(1973);
            }

            [Fact]
            public async Task ShouldInvokeRegisteredQueryHandlerWithCompositeResolver()
            {
                var commandHandler = new TestQueryHandler(_outputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomethingWithNonReferenceTypeResult, int>>(() => commandHandler, Lifestyle.Singleton);
                container.Register<IQueryHandler<QuerySomething, string>>(() => commandHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var containerAsyncHandlerResolver = new ContainerQueryAsyncHandlerResolver(containerAdapter);
                var containerHandlerResolver = new ContainerQueryHandlerResolver(containerAdapter);

                Func<Exception, bool> exceptionHandler = (ex) => 
                {
                    var exception = ex as NoQueryHandlerResolvedException;
                    if (exception != null) 
                    {
                        _outputHelper.WriteLine($"Ignoring encountered exception while trying to resolve query handler for {exception.QueryType.Name}...");
                        
                        // Notify as handled if no command handler is resolved from other resolvers.
                        return true;
                    }

                    return false;
                };

                var compositeResolver = new CompositeQueryHandlerResolver(new List<IQueryHandlerResolver>()
                {
                    containerAsyncHandlerResolver,
                    containerHandlerResolver
                }, exceptionHandler); // Pass in an exception handler.

                var dispatcher = new QueryDispatcher(compositeResolver); // Composite resolver

                int result1 = await dispatcher.DispatchAsync<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(1973));
                string result2 = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething("1973"));
                
                result1.Should().Be(1973);
                result2.Should().Be("1973");
            }

            [Fact]
            public void ShouldThrowWhenNoRegisteredQueryHandlerInContainerIsFound()
            { 
                var container = new Container();
                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryHandlerResolver(containerAdapter); // Sync handler resolver
                var dispatcher = new LoggingQueryDispatcher(new QueryDispatcher(resolver), _outputHelper);
                const string data = nameof(ShouldThrowWhenNoRegisteredAttributeQueryHandlerIsFound);

                Action action = () => dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));

                action.Should().Throw<NoQueryHandlerResolvedException>();
            }

            #endregion Container Registration

            [Fact]
            public void ShouldPropagateExceptionFromQueryHandler()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomethingWithException, string>)new TestQueryHandler(_outputHelper));
                var dispatcher = new LoggingQueryDispatcher(new QueryDispatcher(registration), _outputHelper);

                Action action = () => dispatcher.Dispatch<QuerySomethingWithException, string>(
                    new QuerySomethingWithException("This will cause an exception."));

                action.Should().Throw<TestQueryHandlerException>();
            }
        }

        #endregion Dispatch Method
    }
}
