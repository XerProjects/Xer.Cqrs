using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Events;
using Xer.Cqrs.EventSourcing.DomainEvents;
using Xer.Cqrs.EventSourcing.DomainEvents.Stores;

namespace Xer.Cqrs.EventSourcing.EventStore
{
    public class EventStoreDomainEventStore<TAggregate> : DomainEventAsyncStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        private readonly EventStoreConfiguration _configuration;

        public EventStoreDomainEventStore(EventStoreConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override async Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggregateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = EventStoreConnection.Create(createIpEndpoint(_configuration)))
            {
                await connection.ConnectAsync();

                StreamEventsSlice streamEvents = 
                    await connection.ReadStreamEventsForwardAsync(aggregateId.ToString(), 
                                                                  0, 
                                                                  _configuration.MaxNumberOfStreamEventsToRead, 
                                                                  false, 
                                                                  new UserCredentials(_configuration.Username, 
                                                                                      _configuration.Password));

                List<IDomainEvent> domainEvents = new List<IDomainEvent>(streamEvents.Events.Length);

                foreach(ResolvedEvent streamEvent in streamEvents.Events)
                {
                    string json = Encoding.UTF8.GetString(streamEvent.Event.Data);
                    string metadata = Encoding.UTF8.GetString(streamEvent.Event.Metadata);

                    object data = JsonConvert.DeserializeObject(json,
                                    Type.GetType(metadata, true));

                    DomainEvent domainEvent = data as DomainEvent;
                    if (domainEvent != null)
                    {
                        domainEvents.Add(domainEvent);
                    }
                }

                return new DomainEventStream(aggregateId, domainEvents);
            }
        }

        public override async Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggregateId, int version, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = EventStoreConnection.Create(createIpEndpoint(_configuration)))
            {
                await connection.ConnectAsync();

                StreamEventsSlice streamEvents =
                    await connection.ReadStreamEventsForwardAsync(aggregateId.ToString(),
                                                                  0,
                                                                  1,
                                                                  false,
                                                                  new UserCredentials(_configuration.Username,
                                                                                      _configuration.Password));

                List<IDomainEvent> domainEvents = new List<IDomainEvent>(streamEvents.Events.Length);

                foreach (ResolvedEvent streamEvent in streamEvents.Events)
                {
                    string json = Encoding.UTF8.GetString(streamEvent.Event.Data);
                    string metadata = Encoding.UTF8.GetString(streamEvent.Event.Metadata);
                    object data = JsonConvert.DeserializeObject(
                                    json,
                                    Type.GetType(metadata, true));

                    DomainEvent domainEvent = data as DomainEvent;
                    if (domainEvent != null)
                    {
                        domainEvents.Add(domainEvent);
                    }
                }

                return new DomainEventStream(aggregateId, domainEvents);
            }
        }

        protected override async Task CommitAsync(DomainEventStream domainEventStreamToCommit, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = EventStoreConnection.Create(createIpEndpoint(_configuration)))
            {
                await connection.ConnectAsync();

                foreach (DomainEvent domainEvent in domainEventStreamToCommit)
                {
                    Type domainEventType = domainEvent.GetType();
                    string json = JsonConvert.SerializeObject(domainEvent);
                    byte[] dataBytes = Encoding.UTF8.GetBytes(json);
                    
                    byte[] metadataBytes = Encoding.UTF8.GetBytes(domainEventType.AssemblyQualifiedName);

                    EventData eventData = new EventData(Guid.NewGuid(),
                                                        domainEventType.Name, 
                                                        true, 
                                                        dataBytes,
                                                        metadataBytes);

                    await connection.AppendToStreamAsync(domainEventStreamToCommit.AggregateId.ToString(), 
                                                         ExpectedVersion.Any, 
                                                         new UserCredentials(_configuration.Username, 
                                                                             _configuration.Password), 
                                                         eventData);
                }
            }
        }

        private static IPEndPoint createIpEndpoint(EventStoreConfiguration configuration)
        {
            return new IPEndPoint(IPAddress.Parse(configuration.IpAddress), configuration.Port);
        }
    }
}
