using System;
using System.Collections.Generic;

namespace Xer.Cqrs.EventSourcing.Exceptions
{
    public class DomainEventVersionConflictException : Exception
    {
        public DomainEventVersionConflictException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        public DomainEventVersionConflictException(string message) 
            : this(message, null)
        {
        }
    }
}
