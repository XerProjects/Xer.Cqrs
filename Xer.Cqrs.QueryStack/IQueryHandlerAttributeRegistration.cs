using System;

namespace Xer.Cqrs.QueryStack
{
    public interface IQueryHandlerAttributeRegistration
    {
        /// <summary>
        /// Register methods marked with [QueryHandler] attribute. 
        /// It will make the dispatcher treat the method as command/query handlers.
        /// <para>Supported signatures for [QueryHandler] methods are: (Methods can be named differently)</para>
        /// <para>TResult HandleQuery(TQuery query);</para>
        /// <para>Task&lt;TResult&gt; HandleQueryAsync(TQuery query);</para>
        /// <para>Task&lt;TResult&gt; HandleQueryAsync(TQuery query, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <typeparam name="TAttributed">Type of the objects which contains the attributed methods.</typeparam>
        /// <param name="attributedHandlerFactory">Factory which will create the instance of the TAttributed object.</param>
        void Register<TAttributed>(Func<TAttributed> attributedHandlerFactory) where TAttributed : class;
    }
}
