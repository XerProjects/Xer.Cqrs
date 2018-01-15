using System;
using System.Collections.Generic;
using System.Linq;
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
using Xer.Delegator;

namespace Console
{
    public class App
    {
        private readonly IMessageDelegator _commandDispatcher;
        private readonly IQueryAsyncDispatcher _queryDispatcher;
        private readonly IEventPublisher _eventPublisher;

        private readonly List<IUseCase> _useCases;

        public App(IMessageDelegator commandDispatcher, IQueryAsyncDispatcher queryDispatcher, IEventPublisher eventPublisher)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
            _eventPublisher = eventPublisher;

            _useCases = SetupUseCases();
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
                    await ExecuteUseCase(inputArgs[0], cancellationToken);
                }
                else
                {
                    System.Console.WriteLine("Type any of the following supported use cases:");
                    for(int i = 1; i <= _useCases.Count; i++)
                    {
                        System.Console.WriteLine($"{i}. {_useCases[i - 1].Name}");
                    }
                }
            }
        }

        private Task ExecuteUseCase(string useCaseName, CancellationToken cancellationToken = default(CancellationToken))
        {            
            IUseCase useCase = _useCases.FirstOrDefault(u => string.Equals(u.Name, useCaseName, StringComparison.OrdinalIgnoreCase));
            if(useCase == null)
            {
                System.Console.WriteLine($"Invalid use case: {useCaseName}.");
            }
            
            return useCase.ExecuteAsync(cancellationToken);
        }

        private List<IUseCase> SetupUseCases()
        {
            return new List<IUseCase>()
            {
                // Commands
                new RegisterProductUseCase(_commandDispatcher, _eventPublisher),
                new ActivateProductUseCase(_commandDispatcher),
                new DeactivateProductUseCase(_commandDispatcher),
                // Queries
                new DisplayProductUseCase(_queryDispatcher),
                // Notifications
                new NotifyProductRegisteredUseCase(_eventPublisher, _queryDispatcher)
            };
        }
    }
}