using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Delegator;

namespace Xer.Cqrs.CommandStack
{
    internal class CommandHandlerDelegateBuilder
    {
        #region From CommandHandler

        internal static MessageHandlerDelegate<TCommand> FromCommandHandler<TCommand>(ICommandAsyncHandler<TCommand> commandAsyncHandler)
            where TCommand : class
        {
            return (inputCommand, ct) => commandAsyncHandler.HandleAsync(inputCommand, ct);
        }

        internal static MessageHandlerDelegate<TCommand> FromCommandHandler<TCommand>(ICommandHandler<TCommand> commandHandler)
            where TCommand : class
        {
            return (inputCommand, ct) =>
            {
                try
                {
                    commandHandler.Handle(inputCommand);
                    return TaskUtility.CompletedTask;
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException(ex);
                }
            };
        }

        #endregion From CommandHandler

        #region From Factory
        
        internal static MessageHandlerDelegate<TCommand> FromCommandHandlerFactory<TCommand>(Func<ICommandAsyncHandler<TCommand>> commandHandlerFactory)
            where TCommand : class
        {
            return (inputCommand, ct) =>
            {
                if(!TryGetInstanceFromFactory(commandHandlerFactory, out ICommandAsyncHandler<TCommand> instance, out Exception exception))
                {
                    // Exception occurred or null is returned by factory.
                    return TaskUtility.FromException(exception);
                }
                
                return instance.HandleAsync(inputCommand, ct);
            };
        }

        internal static MessageHandlerDelegate<TCommand> FromCommandHandlerFactory<TCommand>(Func<ICommandHandler<TCommand>> commandHandlerFactory)
            where TCommand : class
        {
            return (inputCommand, ct) =>
            {
                try
                {                    
                    if(!TryGetInstanceFromFactory(commandHandlerFactory, out ICommandHandler<TCommand> instance, out Exception exception))
                    {
                        // Exception occurred or null is returned by factory.
                        return TaskUtility.FromException(exception);
                    }

                    instance.Handle(inputCommand);
                    return TaskUtility.CompletedTask;
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException(ex);
                }
            };
        }

        #endregion From Factory

        #region From Delegate
        
        internal static MessageHandlerDelegate<TCommand> FromDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, Func<TAttributed, TCommand, Task> asyncAction)
            where TAttributed : class
            where TCommand : class
        {
            return (inputCommand, ct) =>
            {
                if(!TryGetInstanceFromFactory(attributedObjectFactory, out TAttributed instance, out Exception exception))
                {
                    // Exception occurred or null is returned by factory.
                    return TaskUtility.FromException(exception);
                }
                
                return asyncAction.Invoke(instance, inputCommand);
            };
        }

        internal static MessageHandlerDelegate<TCommand> FromDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, Func<TAttributed, TCommand, CancellationToken, Task> cancellableAsyncAction)
            where TAttributed : class
            where TCommand : class
        {
            return (inputCommand, ct) =>
            {
                if(!TryGetInstanceFromFactory(attributedObjectFactory, out TAttributed instance, out Exception exception))
                {
                    // Exception occurred or null is returned by factory.
                    return TaskUtility.FromException(exception);
                }

                return cancellableAsyncAction.Invoke(instance, inputCommand, ct);
            };
        }

        internal static MessageHandlerDelegate<TCommand> FromDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, Action<TAttributed, TCommand> action)
            where TAttributed : class
            where TCommand : class
        {
            return (inputCommand, ct) =>
            {
                try
                {
                    if(!TryGetInstanceFromFactory(attributedObjectFactory, out TAttributed instance, out Exception exception))
                    {
                        // Exception occurred or null is returned by factory.
                        return TaskUtility.FromException(exception);
                    }

                    action.Invoke(instance, inputCommand);
                    return TaskUtility.CompletedTask;
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException(ex);
                }
            };
        }

        #endregion From Delegate

        #region Functions

        private static bool TryGetInstanceFromFactory<TInstance>(Func<TInstance> factory, out TInstance instance, out Exception exception) 
            where TInstance : class
        {
            // Defaults.
            instance = null;
            exception = null;

            try
            {
                instance = factory.Invoke();
                if (instance != null)
                {
                    return true;
                }
                
                // Factory returned null, no exception actually occurred.
                exception = FailedToRetrieveInstanceFromFactoryDelegateException<TInstance>();
                return false;
            }
            catch (Exception ex)
            {
                // Wrap inner exception.
                exception = FailedToRetrieveInstanceFromFactoryDelegateException<TInstance>(ex);
                return false;
            }
        }
        
        private static InvalidOperationException FailedToRetrieveInstanceFromFactoryDelegateException<TInstance>(Exception ex = null)
        {
            return new InvalidOperationException($"Failed to retrieve an instance of {typeof(TInstance).Name} from the registered factory delegate. Please check registration configuration.", ex);
        }

        #endregion Functions
    }
}
