using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.AttributeHandlers
{
    public interface IAttributedHandlerRegistration
    {
        /// <summary>
        /// Register methods marked with [CommandHandler] and [QueryHandler] attributes. It will make the dispatcher treat the method as command/query handlers.
        /// <para>Supported signatures for [CommandHandler] methods are: (Methods can be named differently)</para>
        /// <para>void HandleCommand(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
        /// <para>-----------------------------------------------------------------</para>
        /// <para>Supported signatures for [QueryHandler] methods are: (Methods can be named differently)</para>
        /// <para>TResult HandleQuery(TQuery query);</para>
        /// <para>Task&lt;TResult&gt; HandleQueryAsync(TQuery query);</para>
        /// <para>Task&lt;TResult&gt; HandleQueryAsync(TQuery query, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <typeparam name="TAttributed">Type of the objects which contains the attributed methods.</typeparam>
        /// <param name="attributedHandlerFactory">Factory which will create the instance of the TAttributed object.</param>
        void RegisterAttributedMethods<TAttributed>(Func<TAttributed> attributedHandlerFactory);
    }
}
