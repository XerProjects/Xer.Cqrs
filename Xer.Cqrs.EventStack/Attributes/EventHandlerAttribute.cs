using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.EventStack.Attributes
{
    /// <summary>
    /// Methods marked with this attribute will make the event publisher treat the method as an event handler.
    /// <para>Supported method signatures are: (Methods can be named differently)</para>
    /// <para>void HandleEvent(TEvent event);</para>
    /// <para>Task HandleEventAsync(TEvent event);</para>
    /// <para>Task HandleEventAsync(TEvent event, CancellationToken cancellationToken);</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandlerAttribute : Attribute
    {
    }
}
