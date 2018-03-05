using SimpleInjector;
using System.Collections.Generic;
using System.Linq;

namespace Xer.Cqrs.EventStack.Tests.Entities
{
    public class SimpleInjectorContainerAdapter : EventStack.Resolvers.IContainerAdapter
    {
        private readonly Container _container;

        public SimpleInjectorContainerAdapter(Container container)
        {
            _container = container;
        }

        public IEnumerable<T> ResolveMultiple<T>() where T : class
        {
            return _container.GetAllInstances<T>();
        }
    }
}
