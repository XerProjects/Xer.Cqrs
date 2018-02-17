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
        /// <typeparam name="TAttributed">Type to search for methods marked with [CommandHandler] attribute.</param>
        /// <remarks>
        /// This method will search for the methods marked with [CommandHandler] from the type specified in type parameter.
        /// The type parameter should be the actual type that contains [CommandHandler] methods.
        /// </remarks>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="attributedObjectFactory">Factory delegate which provides an instance of a class that contains methods marked with [CommandHandler] attribute.</param>
        public static void RegisterCommandHandlerAttributes<TAttributed>(this SingleMessageHandlerRegistration registration,
                                                                         Func<TAttributed> attributedObjectFactory)
                                                                         where TAttributed : class
        {
            RegisterCommandHandlerAttributes(registration, CommandHandlerAttributeRegistration.ForType<TAttributed>(attributedObjectFactory));
        }

        /// <summary>
        /// Register methods marked with the [CommandHandler] attribute as command handlers.
        /// <para>Supported signatures for methods marked with [CommandHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleCommand(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="registrationInfo">Registration which provides info on a class that contains methods marked with [CommandHandler] attribute.</param>
        public static void RegisterCommandHandlerAttributes(this SingleMessageHandlerRegistration registration,
                                                            CommandHandlerAttributeRegistration registrationInfo)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (registrationInfo == null)
            {
                throw new ArgumentNullException(nameof(registrationInfo));
            }

            // Get all methods marked with CommandHandler attribute and register.
            foreach (CommandHandlerAttributeMethod commandHandlerMethod in CommandHandlerAttributeMethod.FromType(registrationInfo.Type).ToArray())
            {
                // Create method and register to registration.
                RegisterMessageHandlerDelegateOpenGenericMethodInfo
                    .MakeGenericMethod(commandHandlerMethod.CommandType)
                    // Null because this is static method.
                    .Invoke(null, new object[]
                    {
                        registration,
                        commandHandlerMethod,
                        registrationInfo.InstanceFactory
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
            registration.Register<TCommand>(commandHandlerMethod.CreateCommandHandlerDelegate(attributedObjectFactory));
        }

        #endregion Functions
    }

    public class CommandHandlerAttributeRegistration
    {
        /// <summary>
        /// Type to search for methods marked with [CommandHandler] attribute.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Factory delegate that provides an instance of the specified type in the Type property.
        /// </summary>
        public Func<object> InstanceFactory { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Type to search for methods marked with [CommandHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of the specified type.</param>
        private CommandHandlerAttributeRegistration(Type type, Func<object> instanceFactory)
        {
            Type = type;
            InstanceFactory = instanceFactory;
        }      

        /// <summary>
        /// Create registration info for type.
        /// </summary>
        /// <param name="type">Type to search for methods marked with [EventHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of the specified type.</param>
        /// <returns>Irstance of EventHandlerAttributeRegistration for the specified type.</returns>
        public static CommandHandlerAttributeRegistration ForType(Type type, Func<object> instanceFactory)
        {
            return new CommandHandlerAttributeRegistration(type, instanceFactory);
        }

        /// <summary>
        /// Create registration info for type.
        /// </summary>
        /// <typeparam name="T">Type to search for methods marked with [EventHandler] attribute.</typeparam>
        /// <param name="instanceFactory">Factory delegate that provides an instance of the specified type.</param>
        /// <returns>Irstance of EventHandlerAttributeRegistration for the specified type.</returns>
        public static CommandHandlerAttributeRegistration ForType<T>(Func<T> instanceFactory) where T : class
        {
            return new CommandHandlerAttributeRegistration(typeof(T), instanceFactory);
        }
    }
}