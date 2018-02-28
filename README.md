# Build

| Branch | Status |
|--------|--------|
| Master | [![Build status](https://ci.appveyor.com/api/projects/status/y2vr09jehie2yu05/branch/master?svg=true)](https://ci.appveyor.com/project/XerProjects25246/xer-cqrs/branch/master) |
| Dev | [![Build status](https://ci.appveyor.com/api/projects/status/y2vr09jehie2yu05/branch/dev?svg=true)](https://ci.appveyor.com/project/XerProjects25246/xer-cqrs/branch/dev) |


# Table of contents
* [Overview](#overview)
* [Features](#features)
* [Installation](#installation)
* [Getting Started](#getting-started)
   * [Command Handling](#command-handling)
      * [Command Handler Registration](#command-handler-registration)
      * [Command Delegator Usage](#command-delegator-usage)
   * [Event Handling](#event-handling)
      * [Event Handler Registration](#event-handler-registration)
      * [Event Delegator Usage](#event-delegator-usage)
   * [Query Handling](#query-handling)
      * [Query Handler Registration](#query-handler-registration)
      * [Query Dispatcher Usage](#query-dispatcher-usage)

# Overview
Simple CQRS library

This project composes of components for implementing the CQRS pattern (Command Handling, Event Handling, and Query Handling). This library was built with simplicity, modularity and pluggability in mind.

## Features
* Send commands to registered command handlers.
* Send events to registered event handlers.
* Send queries to registered query handler.
* Multiple ways of registering handlers:
    * Simple handler registration (no IoC container).
    * IoC container registration - achieved by creating implementations of IContainerAdapter.
    * Attribute registration - achieved by marking methods with [CommandHandler], [QueryHandler], or [EventHandler] attributes.
* Provides simple abstraction for hosted handlers which can be registered just like an in-process handler.

## Installation
You can simply clone this repository, build the source, reference the dll from the project, and code away!

Xer.Cqrs libraries are also available as Nuget packages:
* https://www.nuget.org/packages/Xer.Cqrs.CommandStack/
* https://www.nuget.org/packages/Xer.Cqrs.QueryStack/
* https://www.nuget.org/packages/Xer.Cqrs.EventStack/

To install Nuget packages:
1. Open command prompt
2. Go to project directory
3. Add the packages to the project:
    ```csharp
    dotnet add package Xer.Cqrs.CommandStack
    ```
    ```csharp
    dotnet add package Xer.Cqrs.EventStack
    ```
    ```csharp
    dotnet add package Xer.Cqrs.QueryStack
    ```
4. Restore the packages:
    ```csharp
    dotnet restore
    ```

## Getting Started
(Samples are in ASP.NET Core)

### Command Handling

```csharp
// Example command.
public class RegisterProductCommand
{
    public int ProductId { get; }
    public string ProductName { get; }

    public RegisterProductCommand(int productId, string productName) 
    {
        ProductId = productId;
        ProductName = productName;
    }
}
```
#### Command Handler Registration

Before we can delegate any commands, first we need to register our command handlers. There are several ways to do this:

##### 1. Simple Registration (No IoC container)
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();

    // Register command delegator.
    services.AddSingleton<CommandDelegator>((serviceProvider) =>
    {
        // Allows registration of a single message handler per message type.
        var registration = new SingleMessageHandlerRegistration();
        registration.RegisterCommandHandler(() => new RegisterProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));

        return new CommandDelegator(registration.BuildMessageHandlerResolver());
    });
    ...
}

// Command handler.
public class RegisterProductCommandHandler : ICommandAsyncHandler<RegisterProductCommand>
{
    private readonly IProductRepository _productRepository;

    public RegisterProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task HandleAsync(RegisterProductCommand command, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _productRepository.SaveAsync(new Product(command.ProductId, command.ProductName));
    }
}
```

##### 2. Container Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();

    // Register command handlers to the container. For this example, I have used a sync command handler.
    // Tip: You can use assembly scanners to scan for command handlers.
    services.AddTransient<ICommandHandler<RegisterProductCommand>, RegisterProductCommandHandler>();

    // Register command handler resolver. This is resolved by the CommandDispatcher.
    services.AddSingleton<CommandDelegator>(serviceProvider =>
        // This ContainerCommandHandlerResolver only resolves sync handlers. 
        // For async handlers, ContainerCommandAsyncHandlerResolver should be used.
        new CommandDelegator(new ContainerCommandHandlerResolver(new AspNetCoreServiceProviderAdapter(serviceProvider)))
    );
    ...
}

// Command handler.
public class RegisterProductCommandHandler : ICommandHandler<RegisterProductCommand>
{
    private readonly IProductRepository _productRepository;

    public RegisterProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public void Handle(RegisterProductCommand command)
    {
        _productRepository.Save(new Product(command.ProductId, command.ProductName));
    }
}

// Container adapter.
class AspNetCoreServiceProviderAdapter : Xer.Cqrs.CommandStack.Resolvers.IContainerAdapter
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
```

##### 3. Attribute Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();

    // Register command delegator.
    services.AddSingleton<CommandDelegator>((serviceProvider) =>
    {
        // Allows registration of a single message handler per message type.
        var registration = new SingleMessageHandlerRegistration();
        // Register methods with [CommandHandler] attribute.
        registration.RegisterCommandHandlerAttributes(() => new RegisterProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));

        return new CommandDelegator(registration.BuildMessageHandlerResolver());
    });
    ...
}

// Command handler.
public class RegisterProductCommandHandler
{
    private readonly IProductRepository _productRepository;

    public RegisterProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    
    [CommandHandler]
    public Task HandleRegisterProductCommandAsync(RegisterProductCommand command, CancellationToken cancellationToken)
    {
        return _productRepository.SaveAsync(new Product(command.ProductId, command.ProductName));
    }
}
```

##### Command Delegator Usage
After setting up the command delegator in the IoC container, commands can now be delegated by simply doing:
```csharp
...
private readonly CommandDelegator _commandDelegator;

public ProductsController(CommandDelegator commandDelegator)
{
    _commandDelegator = commandDelegator;
}

// POST api/products
[HttpPost]
public async Task<IActionResult> RegisterProduct([FromBody]RegisterProductCommandDto model)
{
    RegisterProductCommand command = model.ToDomainCommand();
    await _commandDelegator.SendAsync(command);
    return Accepted();
}
...
```
### Event Handling

```csharp
public class ProductRegisteredEvent
{
    public int ProductId { get; }
    public string ProductName { get; }

    public ProductRegisteredEvent(int productId, string productName)
    {
        ProductId = productId;
        ProductName = productName;
    }
}
```
#### Event Handler Registration

Before we can delegate any events, first, we need to register our event handlers. There are several ways to do this:

##### 1. Simple Registration (No IoC container)
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();

    // Register event delegator.
    services.AddSingleton<EventDelegator>((serviceProvider) =>
    {
        // Allows registration of a multiple message handlers per message type.
        var registration = new MultiMessageHandlerRegistration();
        registration.RegisterEventHandler<ProductRegisteredEvent>(() => new ProductRegisteredEventHandler());
        registration.RegisterEventHandler<ProductRegisteredEvent>(() => new ProductRegisteredEmailNotifier());
        
        return new EventDelegator(registration.BuildMessageHandlerResolver());
    });
    ...
}

// Sync event handler
public class ProductRegisteredEventHandler : IEventHandler<ProductRegisteredEvent>
{
    public void Handle(ProductRegisteredEvent @event)
    {
        System.Console.WriteLine($"ProductRegisteredEventHandler handled {@event.GetType()}.");
    }
}

// Async event handler
public class ProductRegisteredEmailNotifier : IEventAsyncHandler<ProductRegisteredEvent>
{
    public Task HandleAsync(ProductRegisteredEvent @event, CancellationToken ct = default(CancellationToken))
    {
        System.Console.WriteLine($"Sending email notification...");
        return Task.CompletedTask;
    }
}
```

##### 2. Container Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();
    
    // Register event handlers to the container.
    // Tip: You can use assembly scanners to scan for event handlers.
    services.AddTransient<IEventHandler<ProductRegisteredEvent>, ProductRegisteredEventHandler>();
    services.AddTransient<IEventAsyncHandler<ProductRegisteredEvent>, ProductRegisteredEmailNotifier>();

    // Register event delegator.
    services.AddSingleton<EventDelegator>((serviceProvider) =>
        // ContainerEventHandlerResolver resolves async and sync event handlers from the container.
        new EventDelegator(new ContainerEventHandlerResolver(new AspNetCoreServiceProviderAdapter(serviceProvider)))
    );
    ...
}

// Sync event handler 1.
public class ProductRegisteredEventHandler : IEventHandler<ProductRegisteredEvent>
{
    public void Handle(ProductRegisteredEvent @event)
    {
        System.Console.WriteLine($"ProductRegisteredEventHandler handled {@event.GetType()}.");
    }
}

// Async event handler 2.
public class ProductRegisteredEmailNotifier : IEventAsyncHandler<ProductRegisteredEvent>
{
    public Task HandleAsync(ProductRegisteredEvent @event, CancellationToken ct = default(CancellationToken))
    {
        System.Console.WriteLine($"Sending email notification...");
        return Task.CompletedTask;
    }
}

// Container adapter.
class AspNetCoreServiceProviderAdapter : Xer.Cqrs.EventStack.Resolvers.IContainerAdapter
{
    private readonly IServiceProvider _serviceProvider;

    public AspNetCoreServiceProviderAdapter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<T> ResolveMultiple<T>() where T : class;
    {
        return _serviceProvider.GetServices<T>();
    }
}
```

##### 3. Attribute Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();

    // Register event delegator.
    services.AddSingleton<EventDelegator>((serviceProvider) =>
    {
        // Allows registration of a multiple message handlers per message type.
        var registration = new MultiMessageHandlerRegistration();
        // Register all methods with [EventHandler] attribute.
        registration.RegisterEventHandlerAttributes(() => new ProductRegisteredEventHandlers(serviceProvider.GetRequiredService<IProductRepository>()));
        
        return new EventDelegator(registration.BuildMessageHandlerResolver());
    });
    ...
}

public class ProductRegisteredEventHandlers : IEventHandler<ProductRegisteredEvent>
{
    // Sync event handler.
    [EventHandler]
    public void Handle(ProductRegisteredEvent @event)
    {
        System.Console.WriteLine($"ProductRegisteredEventHandler handled {@event.GetType()}.");
    }
    
    // Async event handler.
    [EventHandler]
    public Task SendEmailNotificationAsync(ProductRegisteredEvent @event, CancellationToken ct)
    {
        System.Console.WriteLine($"Sending email notification...");
        return Task.CompletedTask;
    }
}
```
#### Event Delegator Usage
After setting up the event delegator in the Ioc container, events can now be delegated by simply doing:
```csharp
...
private readonly EventDelegator _eventDelegator;

public ProductsController(EventDelegator eventDelegator)
{
    _eventDelegator = eventDelegator;
}

[HttpGet("{productId}")]
public async Task<IActionResult> Notify(ProductRegisteredEventDto model)
{
    await _eventDelegator.SendAsync(new ProductRegisteredEvent(model.ProductId, model.ProductName))
    return Accepted();
}
...
```

### Query Handling

```csharp
// Example query.
public class QueryProductById : IQuery<Product>
{
    public int ProductId { get; }

    public QueryProductById(int productId) 
    {
        ProductId = productId;
    }
}
```

#### Query Handler Registration

Before we can dispatch any queries, first, we need to register our query handlers. There are several ways to do this:

##### 1. Simple Registration (No IoC container)
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Read-side repository.
    services.AddSingleton<IProductReadSideRepository, InMemoryProductReadSideRepository>();

    // Register query dispatcher.
    services.AddSingleton<IQueryAsyncDispatcher>((serviceProvider) =>
    {
        // This object implements IQueryHandlerResolver.
        var registration = new QueryHandlerRegistration();
        registration.Register(() => new QueryProductByIdHandler(serviceProvider.GetRequiredService<IProductReadSideRepository>()));

        return new QueryDispatcher(registration);
    });
    ...
}

// Async query handler.
public class QueryProductByIdHandler : IQueryAsyncHandler<QueryProductById, Product>
{
    private readonly IProductReadSideRepository _productRepository;
    
    public QueryProductByIdHandler(IProductReadSideRepository productRepository)
    {
        _productRepository = productRepository;    
    }

    public Task<Product> HandleAsync(QueryProductById query, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _productRepository.GetProductByIdAsync(query.ProductId);
    }
}
```

##### 2. Container Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Read-side repository.
    services.AddSingleton<IProductReadSideRepository, InMemoryProductReadSideRepository>();
    
    // Register query handlers to the container.
    // Tip: You can use assembly scanners to scan for handlers.
    services.AddTransient<IQueryHandler<QueryProductById, Product>, QueryProductByIdHandler>();

    // Register query dispatcher.
    services.AddSingleton<IQueryAsyncDispatcher>((serviceProvider) =>
        // The ContainerQueryHandlerResolver only resolves sync handlers. 
        // For async handlers, ContainerQueryAsyncHandlerResolver should be used.
        new QueryDispatcher(new ContainerQueryHandlerResolver(new AspNetCoreServiceProviderAdapter(serviceProvider)))
    );
    ...
}

// Sync query handler.
public class QueryProductByIdHandler : IQueryHandler<QueryProductById, Product>
{
    private readonly IProductReadSideRepository _productRepository;
    
    public QueryProductByIdHandler(IProductReadSideRepository productRepository)
    {
        _productRepository = productRepository;    
    }

    public Product Handle(QueryProductById query)
    {
        return _productRepository.GetProductById(query.ProductId);
    }
}

// Container adapter.
class AspNetCoreServiceProviderAdapter : Xer.Cqrs.QueryStack.Resolvers.IContainerAdapter
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
```

##### 3. Attribute Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Read-side repository.
    services.AddSingleton<IProductReadSideRepository, InMemoryProductReadSideRepository>();

    // Register query handler resolver. This is resolved by QueryDispatcher.
    services.AddSingleton<IQueryAsyncDispatcher>((serviceProvider) =>
    {
        // This implements IQueryHandlerResolver.
        var attributeRegistration = new QueryHandlerAttributeRegistration();
        // Register all methods with [QueryHandler] attribute.
        attributeRegistration.Register(() => new QueryProductByIdHandler(serviceProvider.GetRequiredService<IProductReadSideRepository>()));

        return new QueryDispatcher(attributeRegistration);
    });
    ...
}

// Attributed query handler.
public class QueryProductByIdHandler
{
    private readonly IProductReadSideRepository _productRepository;
    
    public QueryProductByIdHandler(IProductReadSideRepository productRepository)
    {
        _productRepository = productRepository;    
    }
    
    [QueryHandler]
    public Product Handle(QueryProductById query)
    {
        return _productRepository.GetProductById(query.ProductId);
    }
}
```
#### Query Dispatcher Usage
After setting up the query dispatcher in the IoC container, queries can now be dispatched by simply doing:
```csharp
...
private readonly IQueryAsyncDispatcher _queryDispatcher;

public ProductsController(IQueryAsyncDispatcher queryDispatcher)
{
    _queryDispatcher = queryDispatcher;
}

[HttpGet("{productId}")]
public async Task<IActionResult> GetProduct(int productId)
{
    Product product = await _queryDispatcher.DispatchAsync<QueryProductById, Product>(new QueryProductById(productId));
    if(product != null)
    {
        return Ok(product);
    }

    return NotFound();
}
...
```
