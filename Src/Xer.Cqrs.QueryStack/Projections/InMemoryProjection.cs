using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack.Projections
{
    public abstract class InMemoryProjection<TId, TViewModel> : Projection<TId, TViewModel> where TViewModel : class
    {   
    }
}
