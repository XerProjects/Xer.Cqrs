using System;

namespace Xer.DomainDriven.Exceptions
{
    public class AggregateNotFoundException : Exception
    {
        public AggregateNotFoundException(Guid aggregateId) : this($"Aggregate with ID {aggregateId} was not found.")
        {
        }

        public AggregateNotFoundException(string message) : base(message)
        {
        }

        public AggregateNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}