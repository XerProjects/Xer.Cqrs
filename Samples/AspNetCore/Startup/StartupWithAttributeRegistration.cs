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
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Delegator;
using Xer.Delegator.Registrations;

namespace AspNetCore
{
    public class StartupWithAttributeRegistration
    {
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
                c.SwaggerDoc("v1", new Info { Title = "AspNetCore Sample", Version = "v1" });
            });

            // Repository.
            services.AddSingleton<IProductRepository, InMemoryProductRepository>();

            // Register command handler resolver. This is resolved by CommandDispatcher.
            services.AddSingleton<IMessageHandlerResolver>((serviceProvider) =>
            {
                // This implements ICommandHandlerResolver.
                var attributeRegistration = new SingleMessageHandlerRegistration();

                // Register methods with [CommandHandler] attribute.
                attributeRegistration.RegisterCommandHandlerAttributes(() => new RegisterProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));
                attributeRegistration.RegisterCommandHandlerAttributes(() => new ActivateProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));
                attributeRegistration.RegisterCommandHandlerAttributes(() => new DeactivateProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));

                return attributeRegistration.BuildMessageHandlerResolver();
            });

            // Command dispatcher.
            services.AddSingleton<IMessageDelegator, MessageDelegator>();

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
    }
}
