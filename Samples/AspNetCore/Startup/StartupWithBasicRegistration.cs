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
    public class StartupWithBasicRegistration
    {
        public StartupWithBasicRegistration(IConfiguration configuration)
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
                c.SwaggerDoc("v1", new Info { Title = "AspNetCore Basic Registration Sample", Version = "v1" });
            });

            // Repository.
            services.AddSingleton<IProductRepository, InMemoryProductRepository>();

            // Register command handler resolver. This is resolved by CommandDispatcher.
            services.AddSingleton<IMessageHandlerResolver>((serviceProvider) =>
            {
                var registration = new SingleMessageHandlerRegistration();
                
                // Needed to cast to ICommandHandler because below handlers implements both ICommandAsyncHandler and ICommandHandler.
                // The Register method accepts both interfaces so compiling is complaining that it is ambiguous.  
                // We could also cast to ICommandAsyncHandler instead but decided to go with this 
                // because I already did the ICommandAsyncHandler in the container registration sample.
                registration.RegisterCommandHandler(() => (ICommandAsyncHandler<RegisterProductCommand>)new RegisterProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));
                registration.RegisterCommandHandler(() => (ICommandAsyncHandler<ActivateProductCommand>)new ActivateProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));
                registration.RegisterCommandHandler(() => (ICommandAsyncHandler<DeactivateProductCommand>)new DeactivateProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));

                return registration.BuildMessageHandlerResolver();
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetCore Basic Registration Sample V1");
            });

            app.UseMvc();
        }
    }
}
