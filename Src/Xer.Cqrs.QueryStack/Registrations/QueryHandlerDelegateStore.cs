using System;
using System.Collections.Generic;

namespace Xer.Cqrs.QueryStack.Registrations
{
    /// <summary>
    /// This class takes care of storing different query delegates with different return types.
    /// </summary>
    internal class QueryHandlerDelegateStore
    {
        public readonly IDictionary<Type, object> _storage = new Dictionary<Type, object>();

        public void Add<TResult>(Type queryType, QueryHandlerDelegate<TResult> queryHandlerDelegate)
        {
            if (queryHandlerDelegate == null)
            {
                throw new ArgumentNullException(nameof(queryHandlerDelegate));
            }

            _storage.Add(queryType, queryHandlerDelegate);
        }

        public bool TryGetValue<TResult>(Type queryType, out QueryHandlerDelegate<TResult> queryHandlerDelegate)
        {
            object value;
            if (_storage.TryGetValue(queryType, out value))
            {
                queryHandlerDelegate = (QueryHandlerDelegate<TResult>)value;
                return true;
            }

            queryHandlerDelegate = default(QueryHandlerDelegate<TResult>);
            return false;
        }
    }
}
