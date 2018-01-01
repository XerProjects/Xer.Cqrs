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
            return new ArgumentException($"Invalid command passed to the command handler delegate. Delegate handles command of type {expected.Name}, but was given a command of type {actual.Name}.");
        }

        internal static NoCommandHandlerResolvedException NoCommandHandlerResolvedException(Type commandType, Exception ex = null)
        {
            if(ex != null)
            {
                return new NoCommandHandlerResolvedException($"Error occurred while trying to resolve command handler to handle command of type: { commandType.Name }.", commandType, ex);
            }
            
            return new NoCommandHandlerResolvedException($"Unable to resolve command handler to handle command of type: { commandType.Name }.", commandType, ex);
        }
    }
}
