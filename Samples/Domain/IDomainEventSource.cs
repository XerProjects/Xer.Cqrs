using System.Collections.Generic;

namespace Domain
{
    public interface IDomainEventSource
    {
        IReadOnlyCollection<IDomainEvent> GetUncommittedDomainEvents();
        void ClearUncommittedDomainEvents();
    }
}