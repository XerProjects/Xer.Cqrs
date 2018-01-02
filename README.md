# Xer.Cqrs
Simple CQRS library

This project composes of components for implementing the CQRS pattern (Command Handling, Query Handling, and Event Handling). This library was built with simplicity, modularity and pluggability in mind.

## Features
* Dispatch commands to their registered command handler (through CommandDispatcher).
* Dispatch queries to their registered query handler (through QueryDispatcher).
* Dispatch events to all registered/subscribed event handlers (through EventPublisher).
* Several ways of registering handlers:
    * Basic handler registration (no IoC container).
    * IoC container registration - achieved by creating implementations of IContainerAdapter or ICommandHandlerResolver/IQueryHandlerResolver/IEventHandlerResolver.
    * Attribute registration - achieved by marking methods with [CommandHandler], [QueryHandler], and [EventHandler] attributes.
* Provides basic abstraction for hosted handlers. They can be registered just like an in-process handler.

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

## Getting started
(Samples are in ASP.NET Core)

### Command Handling

```csharp
// Example command.
public class RegisterProductCommand : Command
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

Before we can dispatch any commands, first we need to register our command handlers. There are several ways to do this:

1. Basic Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();

    // Register command handler resolver. This is resolved by CommandDispatcher.
    services.AddSingleton<ICommandHandlerResolver>((serviceProvider) =>
    {
        // This object implements ICommandHandlerResolver.
        var registration = new CommandHandlerRegistration();
        registration.Register(() => new RegisterProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));

        return registration;
    });

    // Command dispatcher.
    services.AddSingleton<ICommandAsyncDispatcher, CommandDispatcher>();
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

2. Container Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();

    // Register command handlers to the container.
    // You can use assembly scanners to scan for handlers.
    services.AddTransient<ICommandHandler<RegisterProductCommand>, RegisterProductCommandHandler>();

    // Register command handler resolver. This is resolved by the CommandDispatcher.
    services.AddSingleton<ICommandHandlerResolver>(serviceProvider =>
        // This resolver only resolves sync handlers. For async handlers, ContainerCommandAsyncHandlerResolver should be used.
        new ContainerCommandHandlerResolver(new AspNetCoreServiceProviderAdapter(serviceProvider))
    );

    // Register command dispatcher.
    services.AddSingleton<ICommandAsyncDispatcher, CommandDispatcher>();
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

3. Attribute Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();

    // Register command handler resolver. This is resolved by CommandDispatcher.
    services.AddSingleton<ICommandHandlerResolver>((serviceProvider) =>
    {
        // This implements ICommandHandlerResolver.
        var attributeRegistration = new CommandHandlerAttributeRegistration();

        // Register methods with [CommandHandler] attribute.
        attributeRegistration.Register(() => new RegisterProductCommandHandler(serviceProvider.GetRequiredService<IProductRepository>()));

        return attributeRegistration;
    });

    // Command dispatcher.
    services.AddSingleton<ICommandAsyncDispatcher, CommandDispatcher>();
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

##### Command Dispatcher Usage
After setting up the command dispatcher with the command handler registration, commands can now be dispatched by simply doing:
```csharp
...
private readonly ICommandAsyncDispatcher _commandDispatcher;

public ProductsController(ICommandAsyncDispatcher commandDispatcher)
{
    _commandDispatcher = commandDispatcher;
}

// POST api/products
[HttpPost]
public async Task<IActionResult> RegisterProduct([FromBody]RegisterProductCommandDto model)
{
    RegisterProductCommand command = model.ToDomainCommand();
    await _commandDispatcher.DispatchAsync(command);
    return Ok();
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

Before we can dispatch any commands, first, we need to register our query handlers. There are several ways to do this:

1. Basic Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Read-side repository.
    services.AddSingleton<IProductReadSideRepository, InMemoryProductReadSideRepository>();

    // Register query handler resolver. This is resolved by QueryDispatcher.
    services.AddSingleton<IQueryHandlerResolver>((serviceProvider) =>
    {
        // This object implements IQueryHandlerResolver.
        var registration = new QueryHandlerRegistration();
        registration.Register(() => new QueryProductByIdHandler(serviceProvider.GetRequiredService<IProductReadSideRepository>()));

        return registration;
    });

    // Query dispatcher.
    services.AddSingleton<IQueryAsyncDispatcher, QueryDispatcher>();
    ...
}

// Query handler.
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

2. Container Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Read-side repository.
    services.AddSingleton<IProductReadSideRepository, InMemoryProductReadSideRepository>();
    
    // Register query handlers to the container.
    // You can use assembly scanners to scan for handlers.
    services.AddSingleton<IQueryHandler<QueryProductById, Product>, QueryProductByIdHandler>();

    // Register query handler resolver. This is resolved by QueryDispatcher.
    services.AddSingleton<IQueryHandlerResolver>((serviceProvider) =>
        // This resolver only resolves sync handlers. For async handlers, ContainerQueryAsyncHandlerResolver should be used.
        new ContainerQueryHandlerResolver(new AspNetCoreServiceProviderAdapter(serviceProvider))
    );

    // Query dispatcher.
    services.AddSingleton<IQueryAsyncDispatcher, QueryDispatcher>();
    ...
}

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

3. Attribute Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Read-side repository.
    services.AddSingleton<IProductReadSideRepository, InMemoryProductReadSideRepository>();

    // Register query handler resolver. This is resolved by QueryDispatcher.
    services.AddSingleton<IQueryHandlerResolver>((serviceProvider) =>
    {
        // This implements IQueryHandlerResolver.
        var attributeRegistration = new QueryHandlerAttributeRegistration();
        // Register methods with [QueryHandler] attribute.
        attributeRegistration.Register(() => new QueryProductByIdHandler(serviceProvider.GetRequiredService<IProductReadSideRepository>()));

        return attributeRegistration;
    });

    // Query dispatcher.
    services.AddSingleton<IQueryAsyncDispatcher, QueryDispatcher>();
    ...
}

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
After setting up the query dispatcher with the query handler registration, queries can now be dispatched by simply doing:
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

### Event Handling

```csharp
public class ProductRegisteredEvent : IEvent
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

Before we can publish any events, first, we need to register our event handlers. There are several ways to do this:

1. Basic Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();

    // Register command handler resolver. This is resolved by CommandDispatcher.
    services.AddSingleton<IEventHandlerResolver>((serviceProvider) =>
    {
        // This object implements IEventHandlerResolver.
        var basicRegistration = new EventHandlerRegistration();
        
        // Register any implementations of IEventAsyncHandler/IEventHandler
        // which will be invoked when resolved by the EventPublisher.
        basicRegistration.Register<ProductRegisteredEvent>(() => new ProductRegisteredEventHandler());
        basicRegistration.Register<ProductRegisteredEvent>(() => new ProductRegisteredEmailNotifier());
        return basicRegistration;
    });

    // Event publisher.
    services.AddSingleton<IEventPublisher, EventPublisher>();
    ...
}

public class ProductRegisteredEventHandler : IEventHandler<ProductRegisteredEvent>
{
    public void Handle(ProductRegisteredEvent @event)
    {
        System.Console.WriteLine($"ProductRegisteredEventHandler handled {@event.GetType()}.");
    }
}

public class ProductRegisteredEmailNotifier : IEventAsyncHandler<ProductRegisteredEvent>
{
    public Task HandleAsync(ProductRegisteredEvent @event, CancellationToken ct = default(CancellationToken))
    {
        System.Console.WriteLine($"Sending email notification...");
    }
}
```

2. Container Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();
    
    // Register event handlers to the container.
    services.AddTransient<IEventHandler<ProductRegisteredEvent>, ProductRegisteredEventHandler>();
    services.AddTransient<IEventAsyncHandler<ProductRegisteredEvent>, ProductRegisteredEmailNotifier>();

    // Register event handler resolver. This is resolved by EventPublisher.
    services.AddSingleton<IEventHandlerResolver>((serviceProvider) =>
        // This resolver retrieves all async and sync command handlers.
        new ContainerEventHandlerResolver(new AspNetCoreServiceProviderAdapter(serviceProvider))
    );

    // Event publisher.
    services.AddSingleton<IEventPublisher, EventPublisher>();
    ...
}

// Event handler 1.
public class ProductRegisteredEventHandler : IEventHandler<ProductRegisteredEvent>
{
    public void Handle(ProductRegisteredEvent @event)
    {
        System.Console.WriteLine($"ProductRegisteredEventHandler handled {@event.GetType()}.");
    }
}

// Event handler 2.
public class ProductRegisteredEmailNotifier : IEventAsyncHandler<ProductRegisteredEvent>
{
    public Task HandleAsync(ProductRegisteredEvent @event, CancellationToken ct = default(CancellationToken))
    {
        System.Console.WriteLine($"Sending email notification...");
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

3. Attribute Registration
```csharp
private IEventPublisher SetupPublisherWithAttributeRegistration()
{
    // Register any methods marked with [EventHandler] 
    // which will be invoked when resolved by the EventPublisher.
    var attributeRegistration = new EventHandlerAttributeRegistration();
    attributeRegistration.Register(() => new SampleEventHandlerAttributeHandler());
    
    // EventPublisher receives an implementation of IEventHandlerResolver 
    // which is implemented by EventHandlerAttributeRegistration.
    return new EventPublisher(attributeRegistration);
}

// Note: Objects can have multiple [EventHandler] methods for a single event type.
class SampleEventHandlerAttributeHandler
{
    [EventHandler]
    public Task HandleSampleEventAsync(SampleEvent @event, CancellationToken cancellationToken)
    {
        System.Console.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event asynchronously.");
        return Task.CompletedTask;
    }

    [EventHandler]
    public void HandleSampleEvent(SampleEvent @event)
    {
        System.Console.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event synchronously.");
    }
}

// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();

    // Register event handler resolver. This is resolved by EventPublisher.
    services.AddSingleton<IEventHandlerResolver>((serviceProvider) =>
    {
        // This implements IEventHandlerResolver.
        var attributeRegistration = new EventHandlerAttributeRegistration();

        // Register ALL methods with [EventHandler] attribute.
        attributeRegistration.Register(() => new ProductRegisteredEventHandler(serviceProvider.GetRequiredService<IProductRepository>()));
        return attributeRegistration;
    });

    // Event publisher.
    services.AddSingleton<IEventPublisher, EventPublisher>();
    ...
}

// Event handler 1.
[EventHandler]
public class ProductRegisteredEventHandler : IEventHandler<ProductRegisteredEvent>
{
    public void Handle(ProductRegisteredEvent @event)
    {
        System.Console.WriteLine($"ProductRegisteredEventHandler handled {@event.GetType()}.");
    }
}

// Event handler 2.
[EventHandler]
public class ProductRegisteredEmailNotifier : IEventAsyncHandler<ProductRegisteredEvent>
{
    public Task HandleAsync(ProductRegisteredEvent @event, CancellationToken ct = default(CancellationToken))
    {
        System.Console.WriteLine($"Sending email notification...");
    }
}
```
#### Event Publisher Usage
After setting up the event publisher with the event handler registration, events can now be published by simply doing:
```csharp
...
private readonly IEventPublisher _eventPublisher;

public ProductsController(IEventPublisher eventPublisher)
{
    _eventPublisher = eventPublisher;
}

[HttpGet("{productId}")]
public async Task<IActionResult> Notify(ProductRegisteredEventDto model)
{
    await _eventPublisher.PublishAsync(new ProductRegisteredEvent(model.ProductId, model.ProductName))
    return Accepted();
}
...
```
