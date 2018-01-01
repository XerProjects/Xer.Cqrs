using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xer.Cqrs.CommandStack.Attributes;

namespace Xer.Cqrs.CommandStack.Registrations
{
    public class CommandHandlerAttributeRegistration : ICommandHandlerAttributeRegistration, ICommandHandlerResolver
    {
        #region Declarations
        
        private static readonly MethodInfo RegisterCommandHandlerOpenGenericMethodInfo = typeof(CommandHandlerAttributeRegistration)
                                                                                            .GetTypeInfo().
                                                                                            DeclaredMethods.
                                                                                            First(m => m.Name == nameof(registerCommandHandlerMethod));

        private readonly IDictionary<Type, CommandHandlerDelegate> _commandHandlerDelegatesByCommandType = new Dictionary<Type, CommandHandlerDelegate>();

        #endregion Declarations
        
        #region ICommandHandlerAttributeRegistration Implementation

        /// <summary>
        /// Register methods marked with the [CommandHandler] attribute as command handlers.
        /// <para>Supported signatures for methods marked with [CommandHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleCommand(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <typeparam name="TAttributed">Type of the object which contains the methods marked with the [CommandHandler] attribute.</typeparam>
        /// <param name="attributedHandlerFactory">Factory which will provide an instance of the specified <typeparamref name="TAttributed"/> type.</param>
        public void Register<TAttributed>(Func<TAttributed> attributedHandlerFactory) where TAttributed : class
        {
            if(attributedHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedHandlerFactory));
            }

            Type attributedObjectType = typeof(TAttributed);

            // Get all public methods marked with CommandHandler attribute.
            IEnumerable<CommandHandlerAttributeMethod> commandHandlerMethods = CommandHandlerAttributeMethod.FromType(attributedObjectType);

            foreach (CommandHandlerAttributeMethod commandHandlerMethod in commandHandlerMethods)
            {                
                MethodInfo registerCommandHandlerGenericMethodInfo = RegisterCommandHandlerOpenGenericMethodInfo.MakeGenericMethod(
                    attributedObjectType, 
                    commandHandlerMethod.CommandType);

                registerCommandHandlerGenericMethodInfo.Invoke(this, new object[]
                {
                    attributedHandlerFactory, commandHandlerMethod
                });
            }
        }

        #endregion ICommandHandlerAttributeRegistration Implementation

        #region ICommandHandlerResolver Implementation

        /// <summary>
        /// Get the registered command handler delegate which handles the command of the specified type.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <returns>Instance of <see cref="CommandHandlerDelegate"/> which executes the command handler processing.</returns>
        public CommandHandlerDelegate ResolveCommandHandler<TCommand>() where TCommand : class, ICommand
        {
            Type commandType = typeof(TCommand);

            if (!_commandHandlerDelegatesByCommandType.TryGetValue(commandType, out CommandHandlerDelegate commandHandlerDelegate))
            {
                throw ExceptionBuilder.NoCommandHandlerResolvedException(commandType);
            }

            return commandHandlerDelegate;
        }

        #endregion ICommandHandlerResolver Implementation

        #region Functions

        private void registerCommandHandlerMethod<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, CommandHandlerAttributeMethod commandHandlerMethod) 
            where TAttributed : class
            where TCommand : class, ICommand
        {
            Type commandType = typeof(TCommand);

            CommandHandlerDelegate handleCommandDelegate;
            if (_commandHandlerDelegatesByCommandType.TryGetValue(commandType, out handleCommandDelegate))
            {
                throw new InvalidOperationException($"Duplicate command handler registered for {commandType.Name}.");
            }

            CommandHandlerDelegate newHandleCommandDelegate = commandHandlerMethod.CreateDelegate<TAttributed, TCommand>(attributedObjectFactory);

            _commandHandlerDelegatesByCommandType.Add(commandType, newHandleCommandDelegate);
        }

        #endregion Functions
    }
}
