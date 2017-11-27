using System;

namespace Xer.DomainDriven
{
    public interface IAggregate<TId> : IEntity<TId> where TId : IEquatable<TId>
    {
    }
}