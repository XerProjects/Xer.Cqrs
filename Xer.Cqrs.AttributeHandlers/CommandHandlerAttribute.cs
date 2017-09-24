using System;

namespace Xer.Cqrs.AttributeHandlers
{
    /// <summary>
    /// Methods marked with this attribute will make the dispatcher treat the method as a command handler.
    /// <para>Supported method signatures are: (Methods can be named differently)</para>
    /// <para>void HandleCommand(TCommand command);</para>
    /// <para>Task HandleCommandAsync(TCommand command);</para>
    /// <para>Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandHandlerAttribute : Attribute
    {
    }
}
