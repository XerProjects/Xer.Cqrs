using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.Events
{
    public interface IEventHandlerAttributeRegistration
    {
        /// <summary>
        /// Register methods marked with the [EventHandler] attribute as event handlers.
        /// <para>Supported signatures for methods marked with [EventHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleEvent(TEvent event);</para>
        /// <para>Task HandleEventAsync(TEvent event);</para>
        /// <para>Task HandleEventAsync(TEvent event, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <typeparam name="TAttributed">Type of the object which contains the methods marked with the [EventHandler] attribute.</typeparam>
        /// <param name="attributedHandlerFactory">Factory which will provide an instance of the specified <typeparamref name="TAttributed"/> type.</param>
        void Register<TAttributed>(Func<TAttributed> attributedHandlerFactory) where TAttributed : class; 
    }
}
