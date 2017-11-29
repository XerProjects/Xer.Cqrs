using System;
using System.Collections.Generic;

namespace Xer.Cqrs.CommandStack.Resolvers
{
    public class CompositeCommandHandlerResolver : ICommandHandlerResolver
    {
        private readonly IEnumerable<ICommandHandlerResolver> _resolvers;

        public CompositeCommandHandlerResolver(IEnumerable<ICommandHandlerResolver> providers)
        {
            _resolvers = providers;
        }

        /// <summary>
        /// Get the registered command handler which handles the command of the specified type delegate from multiple sources.
        /// </summary><typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <returns>Instance of <see cref="CommandHandlerDelegate"/> which executes the command handler processing.</returns>
        public CommandHandlerDelegate ResolveCommandHandler<TCommand>() where TCommand : class, ICommand
        {
            foreach (ICommandHandlerResolver resolver in _resolvers)
            {
                CommandHandlerDelegate commandHandlerDelegate = resolver.ResolveCommandHandler<TCommand>();
                if (commandHandlerDelegate != null)
                {
                    return commandHandlerDelegate;
                }
            }

            throw new NoCommandHandlerResolvedException($"Unable to resolve command handler to handle command of type: { typeof(TCommand).Name }.");
        }
    }
}
