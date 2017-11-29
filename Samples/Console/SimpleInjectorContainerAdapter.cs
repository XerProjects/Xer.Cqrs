using System.Collections.Generic;
using SimpleInjector;
using Xer.Cqrs.QueryStack.Resolvers;

namespace Console
{
    public class SimpleInjectorContainerAdapter : Xer.Cqrs.CommandStack.Resolvers.IContainerAdapter,
                                                  Xer.Cqrs.QueryStack.Resolvers.IContainerAdapter,
                                                  Xer.Cqrs.Events.Resolvers.IContainerAdapter
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