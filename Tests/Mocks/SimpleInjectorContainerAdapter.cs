using SimpleInjector;
using System.Collections.Generic;
using System.Linq;

namespace Xer.Cqrs.Tests.Mocks
{
    public class SimpleInjectorContainerAdapter : CommandStack.Resolvers.IContainerAdapter, 
                                                  QueryStack.Resolvers.IContainerAdapter, 
                                                  EventStack.Resolvers.IContainerAdapter
    {
        private readonly Container _container;

        public SimpleInjectorContainerAdapter(Container container)
        {
            _container = container;
        }

        public T Resolve<T>() where T : class
        {
            return _container.GetInstance<T>();
        }

        public IEnumerable<T> ResolveMultiple<T>() where T : class
        {
            return _container.GetAllInstances<T>();
        }
    }
}
