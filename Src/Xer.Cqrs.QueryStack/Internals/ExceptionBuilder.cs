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
            return new ArgumentException($"Invalid query passed to the query handler delegate. Delegate handles a {expected.Name} query but was given a {actual.Name} query.");
        }
            
        internal static NoQueryHandlerResolvedException NoQueryHandlerResolvedException(Type queryType, Exception ex = null)
        {
            if(ex != null)
            {
                return new NoQueryHandlerResolvedException($"Error occurred while trying to resolve command handler to handle command of type: { queryType.Name }.", queryType, ex);
            }
            
            return new NoQueryHandlerResolvedException($"Unable to resolve command handler to handle command of type: { queryType.Name }.", queryType);
        }
    }
}
