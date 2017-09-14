using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.Dispatchers.Async
{
    public class CommandAsyncDispatcher : ICommandAsyncDispatcher
    {
        private readonly CommandHandlerRegistration _registration;

        public CommandAsyncDispatcher(CommandHandlerRegistration registration)
        {
            _registration = registration;
        }

        public Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Type comandType = command.GetType();

            IEnumerable<HandleCommandDelegate> handleCommandAsyncDelegates = _registration.GetRegisteredCommandHandlers(comandType);

            List<Task> tasks = new List<Task>(handleCommandAsyncDelegates.Count());

            foreach (HandleCommandDelegate handleCommandDelegate in handleCommandAsyncDelegates)
            {
                Task task = handleCommandDelegate.Invoke(command);
                task.ConfigureAwait(false);

                tasks.Add(task);
            }

            return Task.WhenAll(tasks);
        }
    }
}
