using System;

namespace Xer.Cqrs.CommandStack
{
    internal static class ExceptionBuilder
    {
        internal static InvalidOperationException FailedToRetrieveInstanceFromFactoryDelegateException<TInstance>(Exception ex = null)
        {
            return new InvalidOperationException($"Failed to retrieve an instance of {typeof(TInstance).Name} from the registered factory delegate. Please check registration configuration.", ex);
        }

        internal static ArgumentException InvalidCommandTypeArgumentException(Type expected, Type actual)
        {
            return new ArgumentException($"Invalid command passed to the command handler delegate. Delegate handles command of type {expected.Name}, but was passed in a command of type {actual.Name}.");
        }
    }
}
