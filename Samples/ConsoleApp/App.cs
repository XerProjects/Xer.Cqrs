using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApp.UseCases;
using SimpleInjector;
using Xer.Cqrs.QueryStack;
using Xer.Delegator;

namespace ConsoleApp
{
    public class App
    {
        private readonly Container _container;

        public App(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public async Task StartAsync(string[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            Console.WriteLine("Starting product service...");
            Console.WriteLine("Press Ctrl + C to exit...");

            while(!cancellationToken.IsCancellationRequested)
            {
                // Get registered use cases from container.
                List<IUseCase> useCases = _container.GetAllInstances<IUseCase>().ToList();
                if(useCases.Count == 0)
                {
                    System.Console.WriteLine("No operations are registered.");
                    break;
                }

                // Request input and split.
                Console.WriteLine("Enter operation:");
                string input = Console.ReadLine();

                if (!string.IsNullOrEmpty(input))
                {
                    try
                    {
                        await ExecuteUseCase(input, useCases, cancellationToken);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
                else
                {
                    // Display use cases.
                    DisplayRegisteredOperations(useCases);
                }
            }

            Console.WriteLine("Exiting product service...");
        }

        private Task ExecuteUseCase(string inputOperation, List<IUseCase> useCases, CancellationToken cancellationToken)
        {
            if (int.TryParse(inputOperation, out int index))
            {
                // An index was entered as input.
                if (useCases.Count >= index)
                {
                    return useCases[index - 1].ExecuteAsync(cancellationToken);
                }
            }
            else
            {
                // Use case name was entered as input.
                IUseCase useCase = useCases.FirstOrDefault(u => string.Equals(u.Name, inputOperation, StringComparison.OrdinalIgnoreCase));
                if (useCase != null)
                {
                    return useCase.ExecuteAsync(cancellationToken);
                }
            }

            // Invalid operation. Display valid operations.
            System.Console.WriteLine($"Invalid operation: {inputOperation}");
            DisplayRegisteredOperations(useCases);
            return Task.CompletedTask;
        }

        private static void DisplayRegisteredOperations(List<IUseCase> useCases)
        {
            Console.WriteLine("Type any of the following supported operations:");
            for (int i = 0; i < useCases.Count; i++)
            {
                int displayNumber = i + 1;

                Console.WriteLine($"{displayNumber}. {useCases[i].Name}");
            }
        }
    }
}