using System;
using System.Collections.Generic;
using System.IO;
using Domain;
using Domain.Commands;
using Domain.DomainEvents;
using Infrastructure.DomainEventHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadSide.Products;
using ReadSide.Products.Queries;
using ReadSide.Products.Repositories;
using Swashbuckle.AspNetCore.Swagger;
using Xer.Cqrs;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Resolvers;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Dispatchers;
using Xer.Cqrs.QueryStack.Resolvers;
using Xer.DomainDriven;
using Xer.DomainDriven.Repositories;

namespace AspNetCore
{
    class StartupWithContainerRegistration
    {
        private static readonly string AspNetCoreAppXmlDocPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                                                                    $"{typeof(StartupWithContainerRegistration).Assembly.GetName().Name}.xml");
                                                                    
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
                c.SwaggerDoc("v1", new Info { Title = "AspNetCore Container Registration Sample", Version = "v1" });
                c.IncludeXmlComments(AspNetCoreAppXmlDocPath);
            });

            // Add command and event handlers.
            services.AddCqrs(typeof(RegisterProductCommandHandler).Assembly,
                             typeof(ProductDomainEventsHandler).Assembly);

            // Register query handlers.
            // You can use assembly scanners to scan for handlers.
            services.AddTransient<IQueryAsyncHandler<QueryAllProducts, IReadOnlyCollection<ProductReadModel>>, QueryAllProductsHandler>();
            services.AddTransient<IQueryAsyncHandler<QueryProductById, ProductReadModel>, QueryProductByIdHandler>();

            // Register query dispatcher.
            services.AddSingleton<IQueryAsyncDispatcher>((serviceProvider) =>
                // This resolver only resolves async handlers. For sync handlers, ContainerQueryHandlerResolver should be used.
                new QueryDispatcher(new ContainerQueryAsyncHandlerResolver(serviceProvider.GetRequiredService<AspNetCoreServiceProviderAdapter>()))
            );

            // Write-side repository.
            services.AddSingleton<IAggregateRootRepository<Product>>((serviceProvider) => 
                new PublishingAggregateRootRepository<Product>(new InMemoryAggregateRootRepository<Product>(), 
                                                               serviceProvider.GetRequiredService<IDomainEventPublisher>())
            );

            // Domain event publisher.
            services.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();
            // Read-side repository.
            services.AddSingleton<IProductReadSideRepository, InMemoryProductReadSideRepository>();

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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetCore Container Registration Sample V1");
            });

            app.UseMvc();
        }
    }
}
