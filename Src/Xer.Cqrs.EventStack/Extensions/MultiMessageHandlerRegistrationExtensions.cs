using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack;

namespace Xer.Delegator.Registrations
{
    public static class MultiMessageHandlerRegistrationExtensions
    {
        #region IMessageHandlerRegistration Extensions

        /// <summary>
        /// Register event async handler as subscriber.
        /// </summary>
        /// <typeparam name="TEvent">Type of event to subscribe to.</typeparam>
        /// <param name="registration">Instance of MultiMessageHandlerRegistration.</param>
        /// <param name="eventAsyncHandlerFactory">Factory which will create an instance of an event handler that handles the specified <typeparamref name="TEvent"/> event.</param>
        public static void RegisterEventHandler<TEvent>(this MultiMessageHandlerRegistration registration,
                                                        Func<IEventAsyncHandler<TEvent>> eventAsyncHandlerFactory) 
                                                        where TEvent : class
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (eventAsyncHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(eventAsyncHandlerFactory));
            }

            registration.Register<TEvent>(EventHandlerDelegateBuilder.FromEventHandlerFactory(eventAsyncHandlerFactory));
        }

        /// <summary>
        /// Register event handler as subscriber.
        /// </summary>
        /// <typeparam name="TEvent">Type of event to subscribe to.</typeparam>
        /// <param name="registration">Instance of MultiMessageHandlerRegistration.</param>
        /// <param name="eventHandlerFactory">Factory which will create an instance of an event handler that handles the specified <typeparamref name="TEvent"/> event.</param>
        /// <param name="yieldExecution">True if execution of synchronous handler should yield. Otherwise, false.</param>
        public static void RegisterEventHandler<TEvent>(this MultiMessageHandlerRegistration registration,
                                                        Func<IEventHandler<TEvent>> eventHandlerFactory,
                                                        bool yieldExecution = false) 
                                                        where TEvent : class
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (eventHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerFactory));
            }

            registration.Register<TEvent>(EventHandlerDelegateBuilder.FromEventHandlerFactory(eventHandlerFactory, yieldExecution));
        }

        #endregion IMessageHandlerRegistration Extensions
    }
}