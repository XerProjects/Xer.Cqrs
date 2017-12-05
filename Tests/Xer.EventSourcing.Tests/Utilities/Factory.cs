using System;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Publishers;
using Xer.Cqrs.EventStack.Registrations;
using Xer.EventSourcing.Stores;
using Xer.EventSourcing.Repositories;
using Xer.EventSourcing.Tests.Mocks;

namespace Xer.EventSourcing.Tests.Utilities
{
    public class Factory
    {
        #region CreatePublisher

        public static IEventPublisher CreatePublisher(Action<IEventHandlerRegistration> registrationMutator = null)
        {
            var registration = new EventHandlerRegistration();
            if (registrationMutator != null)
            {
                registrationMutator.Invoke(registration);
            }
            return new EventPublisher(registration);
        }

        #endregion CreatePublisher

        #region CreateEventStore

        public static IDomainEventStore<T, TId> CreateEventStore<T, TId>(IEventPublisher publisher = null) where T : EventSourcedAggregate<TId> 
                                                                                                           where TId : IEquatable<TId>
        {
            return new PublishingDomainEventStore<T, TId>(new InMemoryDomainEventStore<T, TId>(), publisher ?? CreatePublisher());
        }

        public static IDomainEventAsyncStore<T, TId> CreateEventAsyncStore<T, TId>(IEventPublisher publisher = null) where T : EventSourcedAggregate<TId>
                                                                                                                     where TId : IEquatable<TId>
        {
            return new PublishingDomainEventAsyncStore<T, TId>(new InMemoryDomainEventStore<T, TId>(), publisher ?? CreatePublisher());
        }

        #endregion CreateEventStore

        #region CreateRepository

        public static IEventSourcedAggregateRepository<TestAggregate, Guid> CreateTestAggregateRepository(IDomainEventStore<TestAggregate, Guid> domainEventStore)
        {
            return new TestAggregateRepository(domainEventStore);
        }

        public static IEventSourcedAggregateRepository<TestAggregate, Guid> CreateTestAggregateRepository(Action<IEventHandlerRegistration> registrationMutator = null)
        {
            return new TestAggregateRepository(CreateEventStore<TestAggregate, Guid>(CreatePublisher(registrationMutator)));
        }

        public static IEventSourcedAggregateRepository<TestAggregate, Guid> CreateTestAggregateRepository(IEventPublisher publisher)
        {
            return new TestAggregateRepository(CreateEventStore<TestAggregate, Guid>(publisher));
        }
        
        public static IEventSourcedAggregateAsyncRepository<TestAggregate, Guid> CreateTestAggregateAsyncRepository(IDomainEventAsyncStore<TestAggregate, Guid> domainEventStore)
        {
            return new TestAggregateAsyncRepository(domainEventStore);
        }

        public static IEventSourcedAggregateAsyncRepository<TestAggregate, Guid> CreateTestAggregateAsyncRepository(IEventPublisher publisher)
        {
            return new TestAggregateAsyncRepository(CreateEventAsyncStore<TestAggregate, Guid>(publisher));
        }

        public static IEventSourcedAggregateAsyncRepository<TestAggregate, Guid> CreateTestAggregateAsyncRepository(Action<IEventHandlerRegistration> registrationMutator = null) 
        {
            return new TestAggregateAsyncRepository(CreateEventAsyncStore<TestAggregate, Guid>(CreatePublisher(registrationMutator)));
        }

        #endregion CreateRepository
    }
}
