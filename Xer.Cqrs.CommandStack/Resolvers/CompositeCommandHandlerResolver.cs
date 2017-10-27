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
        /// Get the registered command handler delegatefrom multiple sources to handle the command of the specified type.
        /// </summary><typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <returns>Instance of invokeable CommandHandlerDelegate.</returns>
        public CommandHandlerDelegate ResolveCommandHandler<TCommand>() where TCommand : ICommand
        {
            foreach (ICommandHandlerResolver resolver in _resolvers)
            {
                CommandHandlerDelegate handlerDelegate = resolver.ResolveCommandHandler<TCommand>();
                if (handlerDelegate != null)
                {
                    return handlerDelegate;
                }
            }

            throw new CommandNotHandledException($"No command handler is registered to handle command of type: { typeof(TCommand).Name }.");
        }
    }
}
