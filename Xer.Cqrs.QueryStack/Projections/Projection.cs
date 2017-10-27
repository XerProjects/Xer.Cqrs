using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack.Projections
{
    public abstract class Projection<TId, TViewModel> : IProjection<TId, TViewModel>, 
                                                        IEquatable<TViewModel> 
                                                        where TViewModel : class
    {
        public abstract TId ProjectionId { get; }

        public DateTime LastUpdated { get; private set; }

        public abstract Task UpdateAsync(CancellationToken cancellationToken = default(CancellationToken));
        public abstract Task<TViewModel> GetAsync(TId projectionId, CancellationToken cancellationToken = default(CancellationToken));

        public abstract bool Equals(TViewModel other);

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(obj, null))
            {
                return false;
            }

            TViewModel viewModel = obj as TViewModel;
            if(viewModel != null)
            {
                return Equals(viewModel);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
