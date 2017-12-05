using System;
using System.Collections.Generic;

namespace Xer.EventSourcing
{
    public interface IDomainEventStream<TAggregateId> : IEnumerable<IDomainEvent> where TAggregateId : IEquatable<TAggregateId>
    {
        /// <summary>
        /// Id of the aggregate which owns this stream.
        /// </summary>
         TAggregateId AggregateId { get; }
        
        /// <summary>
        /// Aggregate version of the first domain event in this stream.
        /// </summary>
        int BeginVersion { get; }

        /// <summary>
        /// Aggregate version of the last domain event in this stream.
        /// </summary>
        int EndVersion { get; }

        /// <summary>
        /// Get number of domain events in the stream.
        /// </summary>
        int DomainEventCount { get; }
    }
}