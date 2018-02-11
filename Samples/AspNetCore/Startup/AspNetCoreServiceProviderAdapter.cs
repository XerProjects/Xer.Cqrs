using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore
{
    class AspNetCoreServiceProviderAdapter : Xer.Cqrs.CommandStack.Resolvers.IContainerAdapter,
                                             Xer.Cqrs.EventStack.Resolvers.IContainerAdapter,
                                             Xer.Cqrs.QueryStack.Resolvers.IContainerAdapter
    {
        private readonly IServiceProvider _serviceProvider;

        public AspNetCoreServiceProviderAdapter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Resolve<T>() where T : class
        {
            return _serviceProvider.GetService<T>();
        }

        public IEnumerable<T> ResolveMultiple<T>() where T : class
        {
            return _serviceProvider.GetServices<T>();
        }
    }
}