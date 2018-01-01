using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Commands;
using Domain.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Dispatchers;
using Xer.Cqrs.CommandStack.Registrations;
using Xer.Cqrs.CommandStack.Resolvers;

namespace AspNetCore
{
    public class StartupWithMixedRegistration
    {
        public StartupWithMixedRegistration(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {            
            // Swagger.
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "AspNetCore Sample", Version = "v1" });
            });

            // Repository.
            services.AddSingleton<IProductRepository, InMemoryProductRepository>();

            // Register command handlers.
            SetupContainerRegistration(services);
            SetupAttributeRegistration(services);
            SetupBasicRegistration(services);

            // Command dispatcher.
            services.AddSingleton<ICommandAsyncDispatcher>(serviceProvider =>
            {
                // Wrap ASP NET Core service provider in a resolver.
                ICommandHandlerResolver containerResolver = new ContainerCommandAsyncHandlerResolver(new AspNetCoreServiceProviderAdapter(serviceProvider));

                // CommandHandlerAttributeRegistration implements ICommandHandlerResolver.
                ICommandHandlerResolver attributeResolver = serviceProvider.GetRequiredService<CommandHandlerAttributeRegistration>();
                
                // CommandHandlerRegistration implements ICommandHandlerResolver.
                ICommandHandlerResolver basicResolver = serviceProvider.GetRequiredService<CommandHandlerRegistration>();

                // Merge all resolvers.
                var compositeResolver = new CompositeCommandHandlerResolver(new ICommandHandlerResolver[]
                {
                    // Order is followed when resolving handlers.
                    containerResolver,
                    attributeResolver,
                    basicResolver
                }, (ex) => true); // Handle any exception if exception is thrown from a resolver, we can return true to allow the dispatcher to proceed to next resolver.

                return new CommandDispatcher(compositeResolver);
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetCore Sample V1");
            });

            app.UseMvc();
        }

        private static void SetupContainerRegistration(IServiceCollection services)
        {
            // Register handler using container.
            // You can use assembly scanners to scan for handlers.
            services.AddTransient<ICommandHandler<RegisterProductCommand>, RegisterProductCommandHandler>();
        }

        private static void SetupAttributeRegistration(IServiceCollection services)
        {
            services.AddSingleton<CommandHandlerAttributeRegistration>((serviceProvider) =>
            {
                // Register methods marked with [CommandHandler] attribute.
                var attributeRegistration = new CommandHandlerAttributeRegistration();
                attributeRegistration.Register(() => new DeactivateProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));

                return attributeRegistration;
            });
        }

        private static void SetupBasicRegistration(IServiceCollection services)
        {
            services.AddSingleton<CommandHandlerRegistration>((serviceProvider) =>
            {
                // Needed to cast to ICommandHandler because below handlers implements both ICommandAsyncHandler and ICommandHandler.
                // The Register method accepts both interfaces so compiling is complaining that it is ambiguous.  
                var registration = new CommandHandlerRegistration();
                registration.Register(() => (ICommandAsyncHandler<ActivateProductCommand>)new ActivateProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));

                return registration;
            });
        }

        class AspNetCoreServiceProviderAdapter : IContainerAdapter
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
        }
    }
}
