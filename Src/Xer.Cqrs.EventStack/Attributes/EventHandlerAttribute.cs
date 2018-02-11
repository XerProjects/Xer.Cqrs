using System;

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
        /// <summary>
        /// True if synchronous method execution should be yielded. Otherwise, false.
        /// </summary>
        /// <remarks>This option is only applicable to synchronous methods.</remarks>
        public bool YieldSynchronousExecution { get; set; }
    }
}
