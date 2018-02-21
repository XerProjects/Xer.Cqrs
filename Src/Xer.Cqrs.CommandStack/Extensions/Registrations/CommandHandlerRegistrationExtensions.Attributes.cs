using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Attributes;

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
            RegisterCommandHandlerAttributes(registration, CommandHandlerAttributeMethod.FromType<TAttributed>(attributedObjectFactory));
        }

        /// <summary>
        /// Register methods of the specified type that are marked with the [CommandHandler] attribute as command handlers.
        /// <para>Supported signatures for methods marked with [CommandHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleCommand(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="type">Type to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of the specified type.</param>
        public static void RegisterCommandHandlerAttributes(this SingleMessageHandlerRegistration registration,
                                                            Type type, 
                                                            Func<object> instanceFactory)
        {
            RegisterCommandHandlerAttributes(registration, CommandHandlerAttributeMethod.FromType(type, instanceFactory));
        }

        /// <summary>
        /// Register methods of types that are marked with the [CommandHandler] attribute as command handlers.
        /// <para>Supported signatures for methods marked with [CommandHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleCommand(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="types">Types to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of a given type.</param>
        public static void RegisterCommandHandlerAttributes(this SingleMessageHandlerRegistration registration,
                                                            IEnumerable<Type> types, 
                                                            Func<Type, object> instanceFactory)
        {
            RegisterCommandHandlerAttributes(registration, CommandHandlerAttributeMethod.FromTypes(types, instanceFactory));
        }

        /// <summary>
        /// Register methods of types from the assembly that are marked with the [CommandHandler] attribute as command handlers.
        /// <para>Supported signatures for methods marked with [CommandHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleCommand(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="assembly">Assembly to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of a type that has methods marked with [CommandHandler] attribute.</param>
        public static void RegisterCommandHandlerAttributes(this SingleMessageHandlerRegistration registration,
                                                            Assembly assembly, 
                                                            Func<Type, object> instanceFactory)
        {
            RegisterCommandHandlerAttributes(registration, CommandHandlerAttributeMethod.FromAssembly(assembly, instanceFactory));
        }

        /// <summary>
        /// Register methods of types from the list of assemblies that are marked with the [CommandHandler] attribute as command handlers.
        /// <para>Supported signatures for methods marked with [CommandHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleCommand(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="assembly">Assembly to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of a type that has methods marked with [CommandHandler] attribute.</param>
        public static void RegisterCommandHandlerAttributes(this SingleMessageHandlerRegistration registration,
                                                            IEnumerable<Assembly> assemblies, 
                                                            Func<Type, object> instanceFactory)
        {
            RegisterCommandHandlerAttributes(registration, CommandHandlerAttributeMethod.FromAssemblies(assemblies, instanceFactory));
        }

        /// <summary>
        /// Register methods marked with the [CommandHandler] attribute as command handlers.
        /// <para>Supported signatures for methods marked with [CommandHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleCommand(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="commandHandlerMethod">Object which represents a method marked with [CommandHandler] attribute.</param>
        public static void RegisterCommandHandlerAttributes(this SingleMessageHandlerRegistration registration,
                                                            CommandHandlerAttributeMethod commandHandlerMethod)
        {
            RegisterCommandHandlerAttributes(registration, new[] { commandHandlerMethod });
        }

        /// <summary>
        /// Register methods marked with the [CommandHandler] attribute as command handlers.
        /// <para>Supported signatures for methods marked with [CommandHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleCommand(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="commandHandlerMethods">Objects which represent methods marked with [CommandHandler] attribute.</param>
        public static void RegisterCommandHandlerAttributes(this SingleMessageHandlerRegistration registration,
                                                            IEnumerable<CommandHandlerAttributeMethod> commandHandlerMethods)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (commandHandlerMethods == null)
            {
                throw new ArgumentNullException(nameof(commandHandlerMethods));
            }

            // Get all methods marked with CommandHandler attribute and register.
            foreach (CommandHandlerAttributeMethod commandHandlerMethod in commandHandlerMethods)
            {
                // Create method and register to registration.
                RegisterMessageHandlerDelegateOpenGenericMethodInfo
                    .MakeGenericMethod(commandHandlerMethod.CommandType)
                    // Null because this is static method.
                    .Invoke(null, new object[]
                    {
                        registration,
                        commandHandlerMethod
                    });
            }
        }

        #endregion Methods

        #region Functions

        /// <summary>
        /// Create message handler delegate from CommandHandlerAttributeMethod and register to SingleMessageHandlerRegistration.
        /// </summary>
        /// <typeparam name="TCommand">Type of command.</typeparam>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="commandHandlerMethod">Command handler method object built from a method marked with [CommandHandler] attribute.</param>
        private static void registerMessageHandlerDelegate<TCommand>(SingleMessageHandlerRegistration registration,
                                                                     CommandHandlerAttributeMethod commandHandlerMethod)
                                                                     where TCommand : class
        {
            // Create delegate and register.
            registration.Register<TCommand>(commandHandlerMethod.CreateCommandHandlerDelegate());
        }

        #endregion Functions
    }
}