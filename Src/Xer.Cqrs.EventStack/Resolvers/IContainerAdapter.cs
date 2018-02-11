using System.Collections.Generic;

namespace Xer.Cqrs.EventStack.Resolvers
{
    public interface IContainerAdapter
    {
        IEnumerable<T> ResolveMultiple<T>() where T : class;
    }
}