using System;

namespace Xer.Cqrs.QueryStack
{
    internal static class ExceptionBuilder
    {
        internal static InvalidOperationException FailedToRetrieveInstanceFromFactoryDelegateException<T>(Exception innerException = null)
        {
            return new InvalidOperationException($"Failed to retrieve a query handler instance of {typeof(T).Name} from the registered factory.", innerException);
        }

        internal static ArgumentException InvalidQueryTypeArgumentException(Type expected, Type actual)
        {
            return new ArgumentException($"Invalid query passed to the query handler delegate. Delegate handles a {expected.Name} query but was passed in a {actual.Name} query.");
        }
    }
}
