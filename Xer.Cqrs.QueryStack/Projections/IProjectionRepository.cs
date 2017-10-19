using System;
using System.Collections.Generic;

namespace Xer.Cqrs.QueryStack.Projections
{
    public interface IProjectionRepository<TProjection> where TProjection : IProjection
    {
        TProjection GetProjectionById(Guid projectionId);
        IReadOnlyCollection<TProjection> GetAllProjections();
        void SaveProjection(TProjection projection);
    }
}
