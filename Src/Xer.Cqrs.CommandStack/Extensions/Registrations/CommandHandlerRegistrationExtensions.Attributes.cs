using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Resolvers;

namespace Xer.Delegator.Registrations
{
    public static partial class CommandHandlerRegistrationExtensions
    {
        #region Declarations
        
        private static readonly MethodInfo CreateAndRegisterMessageHandlerDelegateOpenGenericMethodInfo = typeof(CommandHandlerRegistrationExtensions)
                                                                                                            .GetTypeInfo()
                                                                                                            .GetDeclaredMethod(nameof(createMessageHandlerDelegate));
        
        #endregion Declarations

        #region Methods
        
        /// <summary>
        /// Register methods marked with the [CommandHandler] attribute as command handlers.
        /// <para>Supported signatures for methods marked with [CommandHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleCommand(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <typeparam name="TAttributedObject">Type of the object which contains the methods marked with the [CommandHandler] attribute.</typeparam>
        /// <param name="attributedHandlerFactory">Factory which will provide an instance of the specified <typeparamref name="TAttributed"/> type.</param>
        public static void RegisterCommandHandlerAttributes<TAttributedObject>(this SingleMessageHandlerRegistration registration, 
                                                                               Func<TAttributedObject> attributedObjectFactory) 
                                                                               where TAttributedObject : class
        {
            List<CommandHandlerAttributeMethod> commandHandlerMethods = CommandHandlerAttributeMethod.FromType(typeof(TAttributedObject));

            foreach (CommandHandlerAttributeMethod commandHandlerMethod in commandHandlerMethods)
            {
                // Create method and register to registration.
                CreateAndRegisterMessageHandlerDelegateOpenGenericMethodInfo
                    .MakeGenericMethod(commandHandlerMethod.DeclaringType, commandHandlerMethod.CommandType)
                    // Null because this is static method.
                    .Invoke(null, new object[] 
                    {
                        registration,
                        commandHandlerMethod,
                        attributedObjectFactory
                    });
            }
        }

        #endregion Methods

        #region Functions

        /// <summary>
        /// Create message handler delegate from CommandHandlerMethod and register to IMessageHandlerRegistration.
        /// </summary>
        /// <typeparam name="TAttributed">Type of the object which contains the methods marked with the [CommandHandler] attribute.</typeparam>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="commandHandlerMethod">Command handler method object built from methods marked with [CommandHandler] attribute.</param>
        /// <param name="attributedHandlerFactory">Factory which will provide an instance of the specified <typeparamref name="TAttributed"/> type.</param>
        private static void createMessageHandlerDelegate<TAttributedObject, TCommand>(SingleMessageHandlerRegistration registration,
                                                                                      CommandHandlerAttributeMethod commandHandlerMethod,
                                                                                      Func<TAttributedObject> attributedObjectFactory)
                                                                                      where TAttributedObject : class
                                                                                      where TCommand : class
        {
            // Create delegate.
            MessageHandlerDelegate<TCommand> commandHandlerDelegate = commandHandlerMethod.CreateMessageHandlerDelegate<TAttributedObject, TCommand>(attributedObjectFactory);

            // Register.
            registration.Register<TCommand>(commandHandlerDelegate);
        }

        #endregion Functions
    }
}