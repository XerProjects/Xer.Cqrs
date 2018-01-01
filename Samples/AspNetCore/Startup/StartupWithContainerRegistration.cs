using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Domain.Commands;
using AspNetCore.Domain.Repositories;
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
    public class StartupWithContainerRegistration
    {
        public StartupWithContainerRegistration(IConfiguration configuration)
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

            // Register command handlers here.
            // You can use assembly scanners to scan for handlers.
            services.AddTransient<ICommandAsyncHandler<RegisterProductCommand>, RegisterProductCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<ActivateProductCommand>, ActivateProductCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<DeactivateProductCommand>, DeactivateProductCommandHandler>();

            // Register command handler resolver. This is resolved by the CommandDispatcher.
            services.AddTransient<ICommandHandlerResolver>(serviceProvider =>
                // This resolver only resolves async handlers. For sync handlers, ContainerCommandHandlerResolver should be used.
                new ContainerCommandAsyncHandlerResolver(new AspNetCoreServiceProviderAdapter(serviceProvider))
            );

            // Register command dispatcher.
            services.AddSingleton<ICommandAsyncDispatcher, CommandDispatcher>();

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
