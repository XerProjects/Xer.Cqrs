using System;
using Xer.Cqrs.Events;
using Xer.Cqrs.Events.Publishers;
using Xer.Cqrs.Events.Registrations;
using Xer.Cqrs.EventSourcing.DomainEvents;
using Xer.Cqrs.EventSourcing.DomainEvents.Stores;
using Xer.Cqrs.EventSourcing.Repositories;
using Xer.Cqrs.EventSourcing.Tests.Mocks;

namespace Xer.Cqrs.EventSourcing.Tests.Utilities
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

        public static IDomainEventStore<T> CreateEventStore<T>(IEventPublisher publisher = null) where T : EventSourcedAggregate
        {
            return new PublishingDomainEventStore<T>(new InMemoryDomainEventStore<T>(), publisher ?? CreatePublisher());
        }

        public static IDomainEventAsyncStore<T> CreateEventAsyncStore<T>(IEventPublisher publisher = null) where T : EventSourcedAggregate
        {
            return new PublishingDomainEventAsyncStore<T>(new InMemoryDomainEventStore<T>(), publisher ?? CreatePublisher());
        }

        #endregion CreateEventStore

        #region CreateRepository

        public static IEventSourcedAggregateRepository<TestAggregate> CreateTestAggregateRepository(IDomainEventStore<TestAggregate> domainEventStore)
        {
            return new TestAggregateRepository(domainEventStore);
        }

        public static IEventSourcedAggregateRepository<TestAggregate> CreateTestAggregateRepository(Action<IEventHandlerRegistration> registrationMutator = null)
        {
            return new TestAggregateRepository(CreateEventStore<TestAggregate>(CreatePublisher(registrationMutator)));
        }

        public static IEventSourcedAggregateRepository<TestAggregate> CreateTestAggregateRepository(IEventPublisher publisher)
        {
            return new TestAggregateRepository(CreateEventStore<TestAggregate>(publisher));
        }
        
        public static IEventSourcedAggregateAsyncRepository<TestAggregate> CreateTestAggregateAsyncRepository(IDomainEventAsyncStore<TestAggregate> domainEventStore)
        {
            return new TestAggregateAsyncRepository(domainEventStore);
        }

        public static IEventSourcedAggregateAsyncRepository<TestAggregate> CreateTestAggregateAsyncRepository(IEventPublisher publisher)
        {
            return new TestAggregateAsyncRepository(CreateEventAsyncStore<TestAggregate>(publisher));
        }

        public static IEventSourcedAggregateAsyncRepository<TestAggregate> CreateTestAggregateAsyncRepository(Action<IEventHandlerRegistration> registrationMutator = null) 
        {
            return new TestAggregateAsyncRepository(CreateEventAsyncStore<TestAggregate>(CreatePublisher(registrationMutator)));
        }

        #endregion CreateRepository
    }
}
