using System;

namespace Xer.Cqrs.QueryStack
{
    public interface IQueryHandlerAttributeRegistration
    {
        /// <summary>
        /// Register methods marked with [QueryHandler] attribute as query handlers.
        /// <para>Supported signatures for [QueryHandler] methods are: (Methods can be named differently)</para>
        /// <para>TResult HandleQuery(TQuery query);</para>
        /// <para>Task&lt;TResult&gt; HandleQueryAsync(TQuery query);</para>
        /// <para>Task&lt;TResult&gt; HandleQueryAsync(TQuery query, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <typeparam name="TAttributed">Type of the object which contains the methods marked with the [QueryHandler] attribute.</typeparam>
        /// <param name="attributedHandlerFactory">Factory which will provide an instance of the specified <typeparamref name="TAttributed"/> type.</param>
        void Register<TAttributed>(Func<TAttributed> attributedHandlerFactory) where TAttributed : class;
    }
}
