using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.QueryStack.Projections;

namespace Xer.Cqrs.Tests.Mocks
{
    public class TestAggregateListViewProjection : IProjection<Guid, string>
    {
        public Guid ProjectionId { get; }
        public DateTime LastUpdated { get; }

        public Task<string> GetAsync(Guid projectionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(string.Empty);
        }

        public Task UpdateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

    }
}
