using System;
using System.Collections.Generic;
using System.Linq;

namespace Xer.Cqrs.QueryStack.Projections
{
    public class InMemoryProjectionRepository<TProjection> : IProjectionRepository<TProjection> where TProjection : IProjection
    {
        private readonly List<TProjection> _projections = new List<TProjection>();

        public IReadOnlyCollection<TProjection> GetAllProjections()
        {
            return _projections;
        }

        public TProjection GetProjectionById(Guid projectionId)
        {
            return _projections.FirstOrDefault(p => p.ProjectionId == projectionId);
        }

        public void SaveProjection(TProjection projection)
        {
            if(_projections.Any(p => p.ProjectionId == projection.ProjectionId))
            {
                _projections.Remove(projection);
            }

            _projections.Add(projection);
        }
    }
}
