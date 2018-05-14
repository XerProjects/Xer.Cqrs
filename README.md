# What is Xer.Cqrs?

Xer.Cqrs is a convenience package that contains all packages needed to build a CQRS write side with DDD concepts. It groups together other smaller XerProjects libraries:
* [Domain Driven](https://github.com/XerProjects/Xer.DomainDriven) - contains Domain Driven Design (DDD) components/concepts.
* [Command Stack](https://github.com/XerProjects/Xer.Cqrs.CommandStack) - contains components for handling commands.
* [Event Stack](https://github.com/XerProjects/Xer.Cqrs.EventStack) - contains components for handling events.

# Build

| Branch | Status |
|--------|--------|
| Master | [![Build status](https://ci.appveyor.com/api/projects/status/jr4h0o8h064m6je2/branch/master?svg=true)](https://ci.appveyor.com/project/XerProjects25246/xer-cqrs-5e3ne/branch/master) |
| Dev | [![Build status](https://ci.appveyor.com/api/projects/status/jr4h0o8h064m6je2/branch/dev?svg=true)](https://ci.appveyor.com/project/XerProjects25246/xer-cqrs-5e3ne/branch/dev) |


# Table of contents
* [Overview](#overview)
* [Features](#features)
* [Installation](#installation)
* [Getting Started](#getting-started)
   * [Command Handling](#command-handling)
   * [Event Handling](#event-handling)

# Overview
Simple CQRS library

This project composes of components for implementing the CQRS pattern (Command Handling, Event Handling) with DDD concepts (Aggregate Roots, Entities, Value Objects, Domain Events). This library was built with simplicity, modularity and pluggability in mind.

## Features
* Send commands to registered command handlers.
* Send events to registered event handlers.
* Provides simple abstraction for hosted command/event handlers which can be registered just like a regular command/event handler.
* Multiple ways of registering command/event handlers:
    * Simple handler registration (no IoC container).
    * IoC container registration
      * achieved by creating implementations of IContainerAdapter or using pre-made extensions pakckages for supported containers. 
    * Attribute registration
      * achieved by marking methods with [CommandHandler] or [EventHandler] attributes from the Xer.Cqrs.CommandStack.Extensions.Attributes and Xer.Cqrs.EventStack.Extensions.Attributes packages.
      
        * Xer.Cqrs.CommandStack.Extensions.Attributes
          * [![NuGet](https://img.shields.io/nuget/v/Xer.Cqrs.CommandStack.Extensions.Attributes.svg)](https://www.nuget.org/packages/Xer.Cqrs.CommandStack.Extensions.Attributes/)
          * See https://github.com/XerProjects/Xer.Cqrs.CommandStack.Extensions.Attributes/blob/dev/README.md for documentation.
        
        * Xer.Cqrs.EventStack.Extensions.Attributes
          * [![NuGet](https://img.shields.io/nuget/v/Xer.Cqrs.EventStack.Extensions.Attributes.svg)](https://www.nuget.org/packages/Xer.Cqrs.EventStack.Extensions.Attributes/)
          * See https://github.com/XerProjects/Xer.Cqrs.EventStack.Extensions.Attributes/blob/dev/README.md for documentation.

## Installation
You can simply clone this repository, build the source, reference the dll from the project, and code away!

Xer.Cqrs is available as a Nuget package:
* [![NuGet](https://img.shields.io/nuget/v/Xer.Cqrs.svg)](https://www.nuget.org/packages/Xer.Cqrs/)

To install Nuget package:
1. Open command prompt
2. Go to project directory
3. Add the packages to the project:
    ```csharp
    dotnet add package Xer.Cqrs
    ```
4. Restore the packages:
    ```csharp
    dotnet restore
    ```

## Getting Started
(Samples are in ASP.NET Core)

### Command Handling
See https://github.com/XerProjects/Xer.Cqrs.CommandStack/blob/dev/README.md for documentation.

### Event Handling
See https://github.com/XerProjects/Xer.Cqrs.EventStack/blob/dev/README.md for documentation.
