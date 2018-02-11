using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
        /// <typeparam name="TAttributedObject">Type of the object which contains the methods marked with the [EventHandler] attribute.</typeparam>
        /// <param name="attributedObjectFactory">Factory which will provide an instance of the specified <typeparamref name="TAttributedObject"/> type.</param>
        public static void RegisterEventHandlerAttributes<TAttributedObject>(this MultiMessageHandlerRegistration registration,
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

            // Get all methods marked with EventHandler attribute and register.
            foreach (EventHandlerAttributeMethod eventHandlerMethod in EventHandlerAttributeMethod.FromType(typeof(TAttributedObject)).ToList())
            {
                // Create method and register to registration.
                CreateAndRegisterMessageHandlerDelegateOpenGenericMethodInfo
                    .MakeGenericMethod(eventHandlerMethod.DeclaringType, eventHandlerMethod.EventType)
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
        /// <typeparam name="TAttributedObject">Type of the object which contains the methods marked with the [EventHandler] attribute.</typeparam>
        /// <typeparam name="TEvent">Type of event.</typeparam>
        /// <param name="registration">Message handler registration.</param>
        /// <param name="eventHandlerMethod">Event handler method object built from methods marked with [EventHandler] attribute.</param>
        /// <param name="attributedHandlerFactory">Factory which will provide an instance of the specified <typeparamref name="TAttributedObject"/> type.</param>
        private static void createMessageHandlerDelegate<TAttributedObject, TEvent>(MultiMessageHandlerRegistration registration,
                                                                                    EventHandlerAttributeMethod eventHandlerMethod,
                                                                                    Func<TAttributedObject> attributedObjectFactory)
                                                                                    where TAttributedObject : class
                                                                                    where TEvent : class
        {
            // Create delegate and register.
            registration.Register<TEvent>(eventHandlerMethod.CreateMessageHandlerDelegate<TAttributedObject, TEvent>(attributedObjectFactory));
        }

        #endregion Functions
    }
}