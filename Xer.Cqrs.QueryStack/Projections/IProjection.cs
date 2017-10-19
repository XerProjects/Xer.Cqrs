using System;

namespace Xer.Cqrs.QueryStack.Projections
{
    public interface IProjection
    {
        Guid ProjectionId { get; }
        DateTime LastUpdated { get; }
    }
}
