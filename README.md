# Xer.Cqrs
Simple CQRS library

This project composes of components for implementing the CQRS pattern (Command Handling, Query Handling, and Event Handling). This library was built with simplicity, modularity and pluggability in mind.

## Features
* Dispatch commands to their registered command handler (through CommandDispatcher).
* Dispatch queries to their registered query handler (through QueryDispatcher).
* Dispatch events to all registered/subscribed event handlers (through EventPublisher).
* Several ways of registering handlers such as basic registration (provided by the library itself), registration through an IoC container, and more.
* Can support multiple IoC containers by creating implementations of IContainerAdapter or ICommandHandlerResolver/IQueryHandlerResolver/IEventHandlerResolver.
* Supports attribute-based handler registrations by marking methods with [CommandHandler], [QueryHandler], and [EventHandler] attributes.

## Installation
You can simply clone this repository and code away!

Xer.Cqrs libraries are also available as Nuget packages:
* https://www.nuget.org/packages/Xer.Cqrs.CommandStack/
* https://www.nuget.org/packages/Xer.Cqrs.QueryStack/
* https://www.nuget.org/packages/Xer.Cqrs.EventStack/

## Getting started

### Command Handling

```csharp
class SampleCommand : Command
{

}
```
#### Command Handler Registration
Basic Registration
```csharp
private CommandDispatcher SetupDispatcherWithBasicRegistration()
{
    // Register any implementations of ICommandAsyncHandler/ICommandHandler
    // which will be invoked when resolved by the CommandDispatcher.
    var registration = new CommandHandlerRegistration();
    registration.Register<SampleCommand>(() => new SampleCommandAsyncHandler());
    
    // Register more command handlers here...

    // CommandDispatcher receives an implementation of ICommandHandlerResolver 
    // which is implemented by CommandHandlerRegistration.
    return new CommandDispatcher(registration);
}

class SampleCommandAsyncHandler : ICommandAsyncHandler<SampleCommand>
{
    public Task HandleAsync(SampleCommand command, CancellationToken cancellationToken = default(CancellationToken))
    {
        System.Console.WriteLine($"{GetType().Name} handled {command.GetType().Name} command.");
        return Task.CompletedTask;
    }
}
```

Container Registration
```csharp
private CommandDispatcher SetupDispatcherWithContainerRegistration()
{
    // Register any command handlers to a container of your choice.
    // In this sample, I've used SimpleInjector.
    var container = new Container();
    container.Register<ICommandHandler<SampleCommand>, SampleCommandHandler>();
    
    // Register more command handlers here...
    
    // ContainerCommandHandlerResolver implements the ICommandHandlerResolver interface 
    // which is used by the CommandDispatcher to resolve command handler instances.
    // To enable ContainerCommandHandlerResolver to resolve command handlers from the container, 
    // an implementation of IContainerAdapter specific to the chosen container library should be created.
    // See https://github.com/jeyjeyemem/Xer.Cqrs/blob/master/Samples/Console/SimpleInjectorContainerAdapter.cs for an example.
    return new CommandDispatcher(new ContainerCommandHandlerResolver(new SimpleInjectorContainerAdapter(container)));
}

class SampleCommandHandler : ICommandHandler<SampleCommand>
{
    public void Handle(SampleCommand command)
    {
        System.Console.WriteLine($"{GetType().Name} handled {command.GetType().Name} command.");
    }
}
```

Attribute Registration
```csharp
private CommandDispatcher SetupDispatcherWithAttributeRegistration()
{
    // Register any methods marked with [CommandHandler] 
    // which will be invoked when resolved by the CommandDispatcher.
    var attributeRegistration = new CommandHandlerAttributeRegistration();
    attributeRegistration.Register(() => new SampleCommandHandlerAttributeHandler());
    
    // Register more objects with methods marked with [CommandHandler] here...

    // CommandDispatcher receives an implementation of ICommandHandlerResolver 
    // which is implemented by CommandHandlerAttributeRegistration.
    return new CommandDispatcher(attributeRegistration);
}

class SampleCommandHandlerAttributeHandler
{
    [CommandHandler]
    public Task HandleSampleCommandAsync(SampleCommand command, CancellationToken cancellationToken)
    {
        System.Console.WriteLine($"{GetType().Name} handled {command.GetType().Name} command.");
        return Task.CompletedTask;
    }
}
```

##### Command Dispatcher Usage
After setting up the command dispatcher with the command handler registration, commands can now be dispatched by simply doing:
```csharp
public async Task Execute()
{
    // Dispatch commands to the registered command handler.
    
    // Displays in console: SampleCommandAsyncHandler handled SampleCommand command.
    await _dispatcherSetupWithBasicRegistration.DispatchAsync(new SampleCommand()); 
    
    // Displays in console: SampleCommandHandler handled SampleCommand command.
    await _dispatcherSetupWithContainerRegistration.DispatchAsync(new SampleCommand());
    
    // Displays in console: SampleCommandHandlerAttributeHandler handled SampleCommand command.
    await _dispatcherSetupWithAttributeRegistration.DispatchAsync(new SampleCommand());
}
```

### Query Handling

```csharp
class SampleQuery : IQuery<SampleResult>
{

}

class SampleResult
{
    public string QueryHandlerName { get; }

    public SampleResult(string queryHandlerName)
    {
        QueryHandlerName = queryHandlerName;
    }
}
```

#### Query Handler Registration
Basic Registration
```csharp
private QueryDispatcher SetupDispatcherWithBasicRegistration()
{
    // Register any implementations of IQueryAsyncHandler/IQueryHandler
    // which will be invoked when resolved by the QueryDispatcher.
    var registration = new QueryHandlerRegistration();
    registration.Register<SampleQuery, SampleResult>(() => new SampleQueryAsyncHandler());
    
    // Register more query handlers here...
    
    // QueryDispatcher receives an implementation of IQueryHandlerResolver 
    // which is implemented by QueryHandlerRegistration.
    return new QueryDispatcher(registration);
}

class SampleQueryAsyncHandler : IQueryAsyncHandler<SampleQuery, SampleResult>
{
    public Task<SampleResult> HandleAsync(SampleQuery query, CancellationToken cancellationToken = default(CancellationToken))
    {
        System.Console.WriteLine($"{GetType().Name} handled {query.GetType().Name} query.");
        return Task.FromResult(new SampleResult(GetType().Name));
    }
}
```

Container Registration
```csharp
private QueryDispatcher SetupDispatcherWithContainerRegistration()
{
    // Register any command handlers to a container of your choice.
    // In this sample, I've used SimpleInjector.
    var container = new Container();
    container.Register<IQueryHandler<SampleQuery, SampleResult>, SampleQueryHandler>();
    
    // ContainerQueryHandlerResolver implements the IQueryHandlerResolver interface 
    // which is used by the QueryDispatcher to resolve query handler instances.
    // To enable ContainerQueryHandlerResolver to resolve query handlers from the container, 
    // an implementation of IContainerAdapter specific to the chosen container library should be created.
    // See https://github.com/jeyjeyemem/Xer.Cqrs/blob/master/Samples/Console/SimpleInjectorContainerAdapter.cs for an example.
    return new QueryDispatcher(new ContainerQueryHandlerResolver(new SimpleInjectorContainerAdapter(container)));
}

class SampleQueryHandler : IQueryHandler<SampleQuery, SampleResult>
{
    public SampleResult Handle(SampleQuery query)
    {
        System.Console.WriteLine($"{GetType().Name} handled {query.GetType().Name} query.");
        return new SampleResult(GetType().Name);
    }
}
```

Attribute Registration
```csharp
private QueryDispatcher SetupDispatcherWithAttributeRegistration()
{
    // Register any methods marked with [QueryHandler] 
    // which will be invoked when resolved by the QueryDispatcher.
    var attributeRegistration = new QueryHandlerAttributeRegistration();
    attributeRegistration.Register(() => new SampleQueryHandlerAttributeHandler());

    // Register more objects with methods marked with [QueryHandler] here...
    
    // QueryDispatcher receives an implementation of IQueryHandlerResolver 
    // which is implemented by QueryHandlerAttributeRegistration.
    return new QueryDispatcher(attributeRegistration);
}

class SampleQueryHandlerAttributeHandler
{
    [QueryHandler]
    public Task<SampleResult> HandleSampleQueryAsync(SampleQuery query, CancellationToken cancellationToken)
    {
        System.Console.WriteLine($"{GetType().Name} handled {query.GetType().Name} query.");
        return Task.FromResult(new SampleResult(GetType().Name));
    }
}
```
#### Query Dispatcher Usage
After setting up the query dispatcher with the query handler registration, queries can now be dispatched by simply doing:
```csharp
public async Task ExecuteDemoAsync()
{
    // Dispatch queries to the registered query handler.
    
    SampleResult result1 = await _dispatcherSetupWithBasicRegistration.DispatchAsync<SampleQuery, SampleResult>(new SampleQuery());
    // Displays the ff in console: 
    // SampleQueryHandler handled SampleQuery query.
    // Received result from SampleQueryHandler.
    System.Console.WriteLine($"Received result from {result1.QueryHandlerName}.");
    
    SampleResult result2 = await _dispatcherSetupWithContainerRegistration.DispatchAsync<SampleQuery, SampleResult>(new SampleQuery());
    // Displays the ff in console: 
    // SampleQueryAsyncHandler handled SampleQuery query.
    // Received result from SampleQueryAsyncHandler.
    System.Console.WriteLine($"Received result from {result2.QueryHandlerName}.");

    SampleResult result3 = await _dispatcherSetupWithAttributeRegistration.DispatchAsync<SampleQuery, SampleResult>(new SampleQuery());
    // Displays the ff in console: 
    // SampleQueryHandlerAttributeHandler handled SampleQuery query.
    // Received result from SampleQueryHandlerAttributeHandler.
    System.Console.WriteLine($"Received result from {result3.QueryHandlerName}.");
}
```

### Event Handling

```csharp
class SampleEvent : IEvent
{

}
```
#### Event Handler Registration
Basic Registration
```csharp
private IEventPublisher SetupPublisherWithBasicRegistration()
{
    // Register any implementations of IEventAsyncHandler/IEventHandler
    // which will be invoked when resolved by the EventPublisher.
    var basicRegistration = new EventHandlerRegistration();
    basicRegistration.Register<SampleEvent>(() => new SampleEventHandler());
    basicRegistration.Register<SampleEvent>(() => new SampleEventAsyncHandler());
    
    // EventPublisher receives an implementation of IEventHandlerResolver 
    // which is implemented by EventHandlerRegistration.
    return new EventPublisher(basicRegistration);
}

class SampleEventHandler : IEventHandler<SampleEvent>
{
    public void Handle(SampleEvent @event)
    {
        System.Console.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event.");
    }
}
```

Container Registration
```csharp
private IEventPublisher SetupPublisherWithContainerRegistration()
{
    // Register all event handlers to a container of your choice.
    // In this sample, I've used SimpleInjector.
    var container = new Container();
    var assemblyContainerEventHandlers = Assembly.GetExecutingAssembly();
    container.RegisterCollection(typeof(IEventHandler<>), assemblyContainerEventHandlers);
    container.RegisterCollection(typeof(IEventAsyncHandler<>), assemblyContainerEventHandlers);

    // ContainerEventHandlerResolver implements the IEventHandlerResolver interface 
    // which is used by the EventPublisher to resolve event handler instances.
    // To enable ContainerEventHandlerResolver to resolve event handlers from the container, 
    // an implementation of IContainerAdapter specific to the chosen container library should be created.
    // See https://github.com/jeyjeyemem/Xer.Cqrs/blob/master/Samples/Console/SimpleInjectorContainerAdapter.cs for an example.
    return new EventPublisher(new ContainerEventHandlerResolver(new SimpleInjectorContainerAdapter(container)));
}

class SampleEventAsyncHandler : IEventAsyncHandler<SampleEvent>
{
    public Task HandleAsync(SampleEvent @event, CancellationToken cancellationToken = default(CancellationToken))
    {
        System.Console.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event.");
        return Task.CompletedTask;
    }
}
```

Attribute Registration
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
```
#### Event Publisher Usage
After setting up the event publisher with the event handler registration, events can now be published by simply doing:
```csharp
public async Task Execute()
{
    // Dispatch event to all registered event handlers.
    await _eventPublisher.PublishAsync(new SampleEvent());
    
    // Displays the ff in console:
    // SampleEventHandler handled SampleEvent event.
    // SampleEventAsyncHandler handled SampleEvent event.
    // SampleEventHandlerAttributeHandler handled SampleEvent event asynchronously.
    // SampleEventHandlerAttributeHandler handled SampleEvent event synchronously.
}
```
