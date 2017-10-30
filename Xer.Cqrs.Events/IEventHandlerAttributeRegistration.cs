using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.Events
{
    public interface IEventHandlerAttributeRegistration
    {
        /// <summary>
        /// Methods marked with this attribute will make the event publisher treat the method as an event handler.
        /// <para>Supported method signatures are: (Methods can be named differently)</para>
        /// <para>void HandleEvent(TEvent command);</para>
        /// <para>Task HandleEventAsync(TEvent command);</para>
        /// <para>Task HandleEventAsync(TEvent command, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <typeparam name="TAttributed">Type of the objects which contains the attributed methods.</typeparam>
        /// <param name="attributedHandlerFactory">Factory which will create the instance of the TAttributed object.</param>
        void Register<TAttributed>(Func<TAttributed> attributedHandlerFactory) where TAttributed : class;
    }
}
