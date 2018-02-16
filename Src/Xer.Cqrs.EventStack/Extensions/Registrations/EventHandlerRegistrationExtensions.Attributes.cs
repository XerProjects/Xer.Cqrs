using System;
using System.Linq;
using System.Reflection;
using Xer.Cqrs.EventStack;

namespace Xer.Delegator.Registrations
{
    public static partial class EventHandlerRegistrationExtensions
    {
        private static readonly MethodInfo CreateAndRegisterMessageHandlerDelegateOpenGenericMethodInfo = typeof(EventHandlerRegistrationExtensions)
                                                                                                            .GetTypeInfo()
                                                                                                            .GetDeclaredMethod(nameof(createMessageHandlerDelegate));

        #region IMessageHandlerRegistration Extensions

        /// <summary>
        /// Register methods marked with the [EventHandler] attribute as event handlers.
        /// <para>Supported signatures for methods marked with [EventHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleEvent(TEvent event);</para>
        /// <para>Task HandleEventAsync(TEvent event);</para>
        /// <para>Task HandleEventAsync(TEvent event, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <remarks>This will try to retrieve an instance from <paramref name="attributedObjectFactory"/> to validate.</remarks>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="attributedObjectFactory">Factory delegate which provides an instance of a class that contains methods marked with [EventHandler] attribute.</param>
        public static void RegisterEventHandlerAttributes(this MultiMessageHandlerRegistration registration,
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

            // Get all methods marked with EventHandler attribute and register.
            foreach (EventHandlerAttributeMethod eventHandlerMethod in EventHandlerAttributeMethod.FromType(attributedObjectType).ToArray())
            {
                // Create method and register to registration.
                CreateAndRegisterMessageHandlerDelegateOpenGenericMethodInfo
                    .MakeGenericMethod(eventHandlerMethod.EventType)
                    // Null because this is static method.
                    .Invoke(null, new object[] 
                    {
                        registration,
                        eventHandlerMethod,
                        attributedObjectFactory
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
        private static void createMessageHandlerDelegate<TEvent>(MultiMessageHandlerRegistration registration,
                                                                 EventHandlerAttributeMethod eventHandlerMethod,
                                                                 Func<object> attributedObjectFactory)
                                                                 where TEvent : class
        {
            // Create delegate and register.
            registration.Register<TEvent>(eventHandlerMethod.CreateEventHandlerDelegate<TEvent>(attributedObjectFactory));
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