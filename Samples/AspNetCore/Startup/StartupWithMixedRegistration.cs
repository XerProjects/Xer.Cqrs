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
using Xer.Cqrs.EventStack;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Dispatchers;
using Xer.Cqrs.QueryStack.Resolvers;
using Xer.Delegator.Registration;
using Xer.DomainDriven;
using Xer.DomainDriven.Repositories;

namespace AspNetCore
{
    class StartupWithMixedRegistration
    {
        private static readonly string AspNetCoreAppXmlDocPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                                                                    $"{typeof(StartupWithMixedRegistration).Assembly.GetName().Name}.xml");

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
                c.SwaggerDoc("v1", new Info { Title = "AspNetCore Mixed Registration Sample", Version = "v1" });
                c.IncludeXmlComments(AspNetCoreAppXmlDocPath);
            });

            // Write-side repository.
            services.AddSingleton<IAggregateRootRepository<Product>>((serviceProvider) => 
                new PublishingAggregateRootRepository<Product>(new InMemoryAggregateRootRepository<Product>(), 
                                                               serviceProvider.GetRequiredService<IDomainEventPublisher>())
            );

            // Domain event publisher.
            services.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();

            // Read-side repository.
            services.AddSingleton<IProductReadSideRepository, InMemoryProductReadSideRepository>();

            // Register command delegator.
            services.AddSingleton<CommandDelegator>((serviceProvider) =>
            {
                // Register command handlers through simple registration.
                var commandHandlerRegistration = new SingleMessageHandlerRegistration();
                commandHandlerRegistration.RegisterCommandHandler(() => new RegisterProductCommandHandler(serviceProvider.GetRequiredService<IAggregateRootRepository<Product>>()));
                commandHandlerRegistration.RegisterCommandHandler(() => new ActivateProductCommandHandler(serviceProvider.GetRequiredService<IAggregateRootRepository<Product>>()));
                commandHandlerRegistration.RegisterCommandHandler(() => new DeactivateProductCommandHandler(serviceProvider.GetRequiredService<IAggregateRootRepository<Product>>()));

                return new CommandDelegator(commandHandlerRegistration.BuildMessageHandlerResolver());
            });

            // Register event delegator.
            services.AddSingleton<EventDelegator>((serviceProvider) =>
            {
                // Register event handlers through simple registration.
                var eventHandlerRegistration = new MultiMessageHandlerRegistration();
                eventHandlerRegistration.RegisterEventHandler<ProductRegisteredEvent>(() => new ProductDomainEventsHandler(serviceProvider.GetRequiredService<IProductReadSideRepository>()));
                eventHandlerRegistration.RegisterEventHandler<ProductActivatedEvent>(() => new ProductDomainEventsHandler(serviceProvider.GetRequiredService<IProductReadSideRepository>()));
                eventHandlerRegistration.RegisterEventHandler<ProductDeactivatedEvent>(() => new ProductDomainEventsHandler(serviceProvider.GetRequiredService<IProductReadSideRepository>()));

                return new EventDelegator(eventHandlerRegistration.BuildMessageHandlerResolver());
            });

            // Register query handlers to container.
            services.AddTransient<IQueryAsyncHandler<QueryAllProducts, IReadOnlyCollection<ProductReadModel>>, QueryAllProductsHandler>();
            services.AddTransient<IQueryAsyncHandler<QueryProductById, ProductReadModel>, QueryProductByIdHandler>();

            // Register query dispatcher.
            services.AddSingleton<IQueryAsyncDispatcher>(serviceProvider =>
                new QueryDispatcher(new ContainerQueryAsyncHandlerResolver(new AspNetCoreServiceProviderAdapter(serviceProvider)))
            );

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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetCore Mixed Registration Sample V1");
            });

            app.UseMvc();
        }
    }
}
