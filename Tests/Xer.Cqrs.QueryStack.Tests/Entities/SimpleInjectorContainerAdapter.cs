using SimpleInjector;
using System.Collections.Generic;
using System.Linq;

namespace Xer.Cqrs.QueryStack.Tests.Entities
{
    public class SimpleInjectorContainerAdapter : QueryStack.Resolvers.IContainerAdapter
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
    }
}
