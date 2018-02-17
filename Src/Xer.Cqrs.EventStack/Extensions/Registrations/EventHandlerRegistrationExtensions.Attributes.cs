using System;
using System.Linq;
using System.Reflection;
using Xer.Cqrs.EventStack;

namespace Xer.Delegator.Registrations
{
    public static partial class EventHandlerRegistrationExtensions
    {
        private static readonly MethodInfo RegisterMessageHandlerDelegateOpenGenericMethodInfo = typeof(EventHandlerRegistrationExtensions)
                                                                                                            .GetTypeInfo()
                                                                                                            .GetDeclaredMethod(nameof(registerMessageHandlerDelegate));

        #region IMessageHandlerRegistration Extensions

        /// <summary>
        /// Register methods marked with the [EventHandler] attribute as event handlers.
        /// <para>Supported signatures for methods marked with [EventHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleEvent(TEvent event);</para>
        /// <para>Task HandleEventAsync(TEvent event);</para>
        /// <para>Task HandleEventAsync(TEvent event, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <typeparam name="TAttributed">Type to search for methods marked with [EventHandler] attribute.</param>
        /// <remarks>
        /// This method will search for the methods marked with [EventHandler] in the type specified in type parameter.
        /// The type parameter should be the actual type that contains [EventHandler] methods.
        /// </remarks>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="attributedObjectFactory">Factory delegate which provides an instance of a class that contains methods marked with [EventHandler] attribute.</param>
        public static void RegisterEventHandlerAttributes<TAttributed>(this MultiMessageHandlerRegistration registration,
                                                                       Func<TAttributed> attributedObjectFactory)
                                                                       where TAttributed : class
        {
            RegisterEventHandlerAttributes(registration, EventHandlerAttributeRegistration.ForType<TAttributed>(attributedObjectFactory));
        }
        
        /// <summary>
        /// Register methods marked with the [EventHandler] attribute as event handlers.
        /// <para>Supported signatures for methods marked with [EventHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleEvent(TEvent event);</para>
        /// <para>Task HandleEventAsync(TEvent event);</para>
        /// <para>Task HandleEventAsync(TEvent event, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="registrationInfo">Registration which provides info about a class that contains methods marked with [EventHandler] attribute.</param>
        public static void RegisterEventHandlerAttributes(this MultiMessageHandlerRegistration registration,
                                                          EventHandlerAttributeRegistration registrationInfo)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (registrationInfo == null)
            {
                throw new ArgumentNullException(nameof(registrationInfo));
            }

            // Get all methods marked with EventHandler attribute and register.
            foreach (EventHandlerAttributeMethod eventHandlerMethod in EventHandlerAttributeMethod.FromType(registrationInfo.Type).ToArray())
            {
                // Create method and register to registration.
                RegisterMessageHandlerDelegateOpenGenericMethodInfo
                    .MakeGenericMethod(eventHandlerMethod.EventType)
                    // Null because this is static method.
                    .Invoke(null, new object[] 
                    {
                        registration,
                        eventHandlerMethod,
                        registrationInfo.InstanceFactory
                    });
            }
        }

        #endregion IMessageHandlerRegistration Extensions

        #region Functions

        /// <summary>
        /// Create message handler delegate from EventHandlerAttributeMethod and register to MultiMessageHandlerRegistration.
        /// </summary>
        /// <typeparam name="TEvent">Type of event.</typeparam>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="eventHandlerMethod">Event handler method object built from methods marked with [EventHandler] attribute.</param>
        /// <param name="attributedObjectFactory">Factory delegate which provides an instance of a class that contains methods marked with [EventHandler] attribute.</param>
        private static void registerMessageHandlerDelegate<TEvent>(MultiMessageHandlerRegistration registration,
                                                                 EventHandlerAttributeMethod eventHandlerMethod,
                                                                 Func<object> attributedObjectFactory)
                                                                 where TEvent : class
        {
            // Create delegate and register.
            registration.Register<TEvent>(eventHandlerMethod.CreateEventHandlerDelegate(attributedObjectFactory));
        }

        #endregion Functions
    }

    public class EventHandlerAttributeRegistration
    {
        /// <summary>
        /// Type to search for methods marked with [EventHandler] attribute.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Factory delegate that provides an instance of the specified type in the Type property.
        /// </summary>
        public Func<object> InstanceFactory { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Type to search for methods marked with [EventHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of the specified type.</param>
        private EventHandlerAttributeRegistration(Type type, Func<object> instanceFactory)
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
        public static EventHandlerAttributeRegistration ForType(Type type, Func<object> instanceFactory)
        {
            return new EventHandlerAttributeRegistration(type, instanceFactory);
        }

        /// <summary>
        /// Create registration info for type.
        /// </summary>
        /// <typeparam name="T">Type to search for methods marked with [EventHandler] attribute.</typeparam>
        /// <param name="instanceFactory">Factory delegate that provides an instance of the specified type.</param>
        /// <returns>Irstance of EventHandlerAttributeRegistration for the specified type.</returns>
        public static EventHandlerAttributeRegistration ForType<T>(Func<T> instanceFactory) where T : class
        {
            return new EventHandlerAttributeRegistration(typeof(T), instanceFactory);
        }
    }
}