using System;
using System.Threading;
using System.Threading.Tasks;
using Console.UseCases;
using Domain;
using Domain.Commands;
using Domain.Queries;
using DomainEvents;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.QueryStack;

namespace Console
{
    public class App
    {
        private readonly ICommandAsyncDispatcher _commandDispatcher;
        private readonly IQueryAsyncDispatcher _queryDispatcher;
        private readonly IEventPublisher _eventPublisher;

        public App(ICommandAsyncDispatcher commandDispatcher, IQueryAsyncDispatcher queryDispatcher, IEventPublisher eventPublisher)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
            _eventPublisher = eventPublisher;
        }

        public async Task StartAsync(string[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            System.Console.WriteLine("Starting product service...");

            while(!cancellationToken.IsCancellationRequested)
            {
                System.Console.WriteLine("Enter operation:");
                string input = System.Console.ReadLine();

                string[] inputArgs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if(inputArgs.Length != 0)
                {
                    string operation = inputArgs[0];

                    if(string.Equals(operation, "exit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    else if(string.Equals(operation, "command", StringComparison.OrdinalIgnoreCase))
                    {
                        await ProcessCommand(inputArgs, cancellationToken);
                    }
                    else if(string.Equals(operation, "query", StringComparison.OrdinalIgnoreCase))
                    {
                        await ProcessQuery(inputArgs, cancellationToken);
                    }
                    else if(string.Equals(operation, "notify", StringComparison.OrdinalIgnoreCase))
                    {
                        await ProcessEvent(inputArgs, cancellationToken);
                    }
                }
            }
        }

        private async Task ProcessEvent(string[] inputArgs, CancellationToken cancellationToken)
        {
            string evenntToPublish = inputArgs[1] ?? throw new ArgumentException("No event to publish.");

            if (evenntToPublish == "registered")
            {
                await _eventPublisher.PublishAsync(new ProductRegisteredEvent(1, "Test Product"));
            }
        }

        private Task ProcessQuery(string[] inputArgs, CancellationToken cancellationToken)
        {
            string queryToExecute = inputArgs[1] ?? throw new ArgumentException("No query to execute.");

            if (queryToExecute == "product")
            {
                return ExecuteUseCase(new DisplayProductUseCase(_queryDispatcher));
            }

            System.Console.WriteLine("Invalid query.");

            return Task.CompletedTask;
        }

        private Task ProcessCommand(string[] inputArgs, CancellationToken cancellationToken)
        {
            string commandToExecute = inputArgs[1] ?? throw new ArgumentException("No command to execute.");

            if (commandToExecute == "register")
            {
                return ExecuteUseCase(new RegisterProductUseCase(_commandDispatcher, _eventPublisher));
            }
            else if (commandToExecute == "activate")
            {
                return ExecuteUseCase(new ActivateProductUseCase(_commandDispatcher));
            }
            else if (commandToExecute == "deactivate")
            {
                return ExecuteUseCase(new DeactivateProductUseCase(_commandDispatcher), cancellationToken);
            }

            System.Console.WriteLine("Invalid command.");

            return Task.CompletedTask;
        }

        private Task ExecuteUseCase(IUseCase useCase, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (useCase == null)
            {
                throw new ArgumentNullException(nameof(useCase));
            }

            return useCase.ExecuteAsync(cancellationToken);
        }
    }
}