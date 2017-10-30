using System;

namespace Xer.Cqrs.Events
{
    internal static class ExceptionBuilder
    {
        internal static InvalidOperationException FailedToRetrieveInstanceFromFactoryDelegateException<T>(Exception innerException = null)
        {
            return new InvalidOperationException($"Failed to retrieve an event handler instance from the registered factory for {typeof(T).Name}.", innerException);
        }

        internal static ArgumentException InvalidEventTypeArgumentException(Type expected, Type actual)
        {
            return new ArgumentException($"Invalid event passed to the event handler delegate. Delegate handles a {expected.Name} event but was passed in a {actual.Name} event.");
        }
    }
}
