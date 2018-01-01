using System;
using System.Collections.Generic;

namespace Xer.Cqrs.CommandStack.Resolvers
{
    public class CompositeCommandHandlerResolver : ICommandHandlerResolver
    {
        private readonly IEnumerable<ICommandHandlerResolver> _resolvers;
        private readonly Func<Exception, bool> _exceptionHandler;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resolvers">List of resolvers.</param>
        public CompositeCommandHandlerResolver(IEnumerable<ICommandHandlerResolver> resolvers)
        {
            _resolvers = resolvers;
        }

        /// <summary>
        /// Constructor with exception handler.
        /// </summary>
        /// <param name="resolvers">List of resolvers.</param>
        /// <param name="exceptionHandler">
        /// If exception handler returns true, this resolver will try to resolve a command handler 
        /// from the next resolver in the list. Otherwise, resolve will stop and exception will be re-thrown.
        /// </param>
        public CompositeCommandHandlerResolver(IEnumerable<ICommandHandlerResolver> resolvers, Func<Exception, bool> exceptionHandler)
        {
            _resolvers = resolvers;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Get the registered command handler which handles the command of the specified type delegate from multiple sources.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <returns>Instance of <see cref="CommandHandlerDelegate"/> which executes the command handler processing.</returns>
        public CommandHandlerDelegate ResolveCommandHandler<TCommand>() where TCommand : class, ICommand
        {
            foreach (ICommandHandlerResolver resolver in _resolvers)
            {
                try
                {
                    CommandHandlerDelegate commandHandlerDelegate = resolver.ResolveCommandHandler<TCommand>();
                    if (commandHandlerDelegate != null)
                    {
                        return commandHandlerDelegate;
                    }
                }
                catch(Exception ex)
                {
                    if(_exceptionHandler == null)
                    {
                        // No exception handler. Re-throw exception.
                        throw;
                    }

                    bool handled = _exceptionHandler.Invoke(ex);
                    if(!handled)
                    {
                        // Not handled. Re-throw exception.
                        throw;
                    }
                }
            }
            
            throw ExceptionBuilder.NoCommandHandlerResolvedException(typeof(TCommand));
        }
    }
}
