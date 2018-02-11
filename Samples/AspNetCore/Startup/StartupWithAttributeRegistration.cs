using System;
using System.IO;
using Domain.Commands;
using Domain.Repositories;
using Infrastructure.DomainEventHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadSide.Products.Queries;
using ReadSide.Products.Repositories;
using Swashbuckle.AspNetCore.Swagger;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Dispatchers;
using Xer.Cqrs.QueryStack.Registrations;
using Xer.Delegator;
using Xer.Delegator.Registrations;
using Xer.Delegator.Resolvers;

namespace AspNetCore
{
    class StartupWithAttributeRegistration
    {
        private static readonly string AspNetCoreAppXmlDocPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                                                                    $"{typeof(StartupWithAttributeRegistration).Assembly.GetName().Name}.xml");
        public StartupWithAttributeRegistration(IConfiguration configuration)
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
                c.SwaggerDoc("v1", new Info { Title = "AspNetCore Attribute Registration Sample", Version = "v1" });
                c.IncludeXmlComments(AspNetCoreAppXmlDocPath);
            });

            // Write-side repository.
            services.AddSingleton<IProductRepository>((serviceProvider) => 
                new PublishingProductRepository(new InMemoryProductRepository(), serviceProvider.GetRequiredService<IEventDelegator>())
            );

            // Read-side repository.
            services.AddSingleton<IProductReadSideRepository, InMemoryProductReadSideRepository>();

            // Register command delegator.
            services.AddSingleton<ICommandDelegator>((serviceProvider) =>
            {
                // Register methods with [CommandHandler] attribute.
                var commandHandlerAttributeRegistration = new SingleMessageHandlerRegistration();
                commandHandlerAttributeRegistration.RegisterCommandHandlerAttributes(() => new RegisterProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));
                commandHandlerAttributeRegistration.RegisterCommandHandlerAttributes(() => new ActivateProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));
                commandHandlerAttributeRegistration.RegisterCommandHandlerAttributes(() => new DeactivateProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));

                return new CommandDelegator(commandHandlerAttributeRegistration.BuildMessageHandlerResolver());
            });

            // Register event delegator.
            services.AddSingleton<IEventDelegator>((serviceProvider) =>
            {
                // Register methods with [EventHandler] attribute.
                var eventHandlerAttributeRegistration = new MultiMessageHandlerRegistration();
                eventHandlerAttributeRegistration.RegisterEventHandlerAttributes(() => new ProductDomainEventsHandler(serviceProvider.GetRequiredService<IProductReadSideRepository>()));
            
                return new EventDelegator(eventHandlerAttributeRegistration.BuildMessageHandlerResolver());
            });

            // Register query dispatcher.
            services.AddSingleton<IQueryAsyncDispatcher>((serviceProvider) =>
            {
                // Register methods with [QueryHandler] attribute.
                var attributeRegistration = new QueryHandlerAttributeRegistration();
                attributeRegistration.Register(() => new QueryAllProductsHandler(serviceProvider.GetRequiredService<IProductReadSideRepository>()));
                attributeRegistration.Register(() => new QueryProductByIdHandler(serviceProvider.GetRequiredService<IProductReadSideRepository>()));

                return new QueryDispatcher(attributeRegistration);
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetCore Attribute Registration Sample V1");
            });

            app.UseMvc();
        }
    }
}
