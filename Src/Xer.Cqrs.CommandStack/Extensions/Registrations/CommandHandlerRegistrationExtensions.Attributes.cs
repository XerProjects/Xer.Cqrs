using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack;

namespace Xer.Delegator.Registrations
{
    public static partial class CommandHandlerRegistrationExtensions
    {
        #region Declarations
        
        private static readonly MethodInfo RegisterMessageHandlerDelegateOpenGenericMethodInfo = typeof(CommandHandlerRegistrationExtensions)
                                                                                                            .GetTypeInfo()
                                                                                                            .GetDeclaredMethod(nameof(registerMessageHandlerDelegate));
        
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
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (attributedObjectFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedObjectFactory));
            }

            // Get all methods marked with CommandHandler attribute and register.
            foreach (CommandHandlerAttributeMethod commandHandlerMethod in CommandHandlerAttributeMethod.FromType(typeof(TAttributedObject)).ToList())
            {
                // Create method and register to registration.
                RegisterMessageHandlerDelegateOpenGenericMethodInfo
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
        /// Create message handler delegate from CommandHandlerAttributeMethod and register to SingleMessageHandlerRegistration.
        /// </summary>
        /// <typeparam name="TAttributedObject">Type of the object which contains the methods marked with the [CommandHandler] attribute.</typeparam>
        /// <typeparam name="TEvent">Type of event.</typeparam>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="commandHandlerMethod">Command handler method object built from methods marked with [CommandHandler] attribute.</param>
        /// <param name="attributedHandlerFactory">Factory which will provide an instance of the specified <typeparamref name="TAttributedObject"/> type.</param>
        private static void registerMessageHandlerDelegate<TAttributedObject, TCommand>(SingleMessageHandlerRegistration registration,
                                                                                        CommandHandlerAttributeMethod commandHandlerMethod,
                                                                                        Func<TAttributedObject> attributedObjectFactory)
                                                                                        where TAttributedObject : class
                                                                                        where TCommand : class
        {
            // Create delegate and register.
            registration.Register<TCommand>(commandHandlerMethod.CreateMessageHandlerDelegate<TAttributedObject, TCommand>(attributedObjectFactory));
        }

        #endregion Functions
    }
}