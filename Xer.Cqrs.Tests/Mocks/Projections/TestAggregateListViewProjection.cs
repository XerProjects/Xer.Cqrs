using System;
using Xer.Cqrs.QueryStack.Projections;

namespace Xer.Cqrs.Tests.Mocks.Projections
{
    public class TestAggregateListViewProjection : IProjection
    {
        public Guid ProjectionId { get; }
        public DateTime LastUpdated { get; }
    }
}
