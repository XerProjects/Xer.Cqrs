using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp.UseCases
{
    public abstract class UseCaseBase : IUseCase
    {
        public abstract string Name { get; }

        protected string RequestInput(string message, Func<string, InputValidationResult> inputValidator = null)
        {
            System.Console.WriteLine(message);
            string input = System.Console.ReadLine();

            if (inputValidator != null)
            {
                while (true)
                {
                    InputValidationResult result = inputValidator.Invoke(input);
                    if (!result.IsSuccessful)
                    {
                        System.Console.WriteLine("Input error:");
                        foreach (string error in result.ErrorMessages)
                        {
                            System.Console.WriteLine($"- {error}");
                        }

                        System.Console.WriteLine(message);
                        input = System.Console.ReadLine();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return input;
        }

        public abstract Task ExecuteAsync(CancellationToken cancellationToken = default(CancellationToken));

        protected class InputValidationResult
        {
            public static readonly InputValidationResult Success = new InputValidationResult(true, string.Empty);

            public bool IsSuccessful { get; private set; }
            public string[] ErrorMessages { get; private set; }

            private InputValidationResult(bool isSuccessful, params string[] errorMessages)
            {
                IsSuccessful = isSuccessful;
                ErrorMessages = errorMessages;
            }

            public static InputValidationResult WithErrors(params string[] errorMessages)
            {
                return new InputValidationResult(false, errorMessages);
            }
        }
    }
}