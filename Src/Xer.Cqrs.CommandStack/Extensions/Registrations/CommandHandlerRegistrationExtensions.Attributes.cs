using System;
using System.Linq;
using System.Reflection;
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
        /// <remarks>This will try to retrieve an instance from <paramref name="attributedObjectFactory"/> to validate.</remarks>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="attributedObjectFactory">Factory which provides an instance of a class that contains methods marked with [CommandHandler] attribute.</param>
        public static void RegisterCommandHandlerAttributes(this SingleMessageHandlerRegistration registration,
                                                            Func<object> attributedObjectFactory)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (attributedObjectFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedObjectFactory));
            }

            // Will throw if no instance was retrieved.
            Type attributedObjectType = getInstanceType(attributedObjectFactory);

            // Get all methods marked with CommandHandler attribute and register.
            foreach (CommandHandlerAttributeMethod commandHandlerMethod in CommandHandlerAttributeMethod.FromType(attributedObjectType).ToArray())
            {
                // Create method and register to registration.
                RegisterMessageHandlerDelegateOpenGenericMethodInfo
                    .MakeGenericMethod(commandHandlerMethod.CommandType)
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
        /// <typeparam name="TEvent">Type of event.</typeparam>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="commandHandlerMethod">Command handler method object built from a method marked with [CommandHandler] attribute.</param>
        /// <param name="attributedObjectFactory">Factory which provides an instance of a class that contains methods marked with [CommandHandler] attribute.</param>
        private static void registerMessageHandlerDelegate<TCommand>(SingleMessageHandlerRegistration registration,
                                                                     CommandHandlerAttributeMethod commandHandlerMethod,
                                                                     Func<object> attributedObjectFactory)
                                                                     where TCommand : class
        {
            // Create delegate and register.
            registration.Register<TCommand>(commandHandlerMethod.CreateCommandHandlerDelegate<TCommand>(attributedObjectFactory));
        }

        /// <summary>
        /// Vaidate and get the type if the instance produced by the instance factory delegate.
        /// </summary>
        /// <param name="attributedObjectFactory">Factory which provides an instance of a class that contains methods marked with [CommandHandler] attribute.</param>
        /// <returns>Type of the instance returned by the instance factory delegate.</returns>
        private static Type getInstanceType(Func<object> attributedObjectFactory)
        {
            Type attributedObjectType;

            try
            {
                var instance = attributedObjectFactory.Invoke();
                if (instance == null)
                {
                    throw new ArgumentException($"Failed to retrieve an instance from the provided instance factory delegate. Please check registration configuration.");
                }

                // Get actual type.
                attributedObjectType = instance.GetType();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error occurred while trying to retrieve an instance from the provided instance factory delegate. Please check registration configuration.", ex);
            }

            return attributedObjectType;
        }

        #endregion Functions
    }
}