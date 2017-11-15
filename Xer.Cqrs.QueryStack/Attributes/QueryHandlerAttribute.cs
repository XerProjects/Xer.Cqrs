using System;

namespace Xer.Cqrs.QueryStack
{
    /// <summary>
    /// Methods marked with this attribute will make the dispatcher treat the method as a query handler.
    /// <para>Supported method signatures are: (Methods can be named differently)</para>
    /// <para>TResult HandleQuery(TQuery query);</para>
    /// <para>Task&lt;TResult&gt; HandleQueryAsync(TQuery query);</para>
    /// <para>Task&lt;TResult&gt; HandleQueryAsync(TQuery query, CancellationToken cancellationToken);</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class QueryHandlerAttribute : Attribute
    {
    }
}
