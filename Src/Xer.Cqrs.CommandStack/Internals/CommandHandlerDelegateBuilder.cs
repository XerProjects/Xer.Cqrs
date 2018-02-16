using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Delegator;

namespace Xer.Cqrs.CommandStack
{
    internal class CommandHandlerDelegateBuilder
    {
        #region From CommandHandler

        internal static MessageHandlerDelegate FromCommandHandler<TCommand>(ICommandAsyncHandler<TCommand> commandAsyncHandler)
            where TCommand : class
        {
            if (commandAsyncHandler == null)
            {
                throw new ArgumentNullException(nameof(commandAsyncHandler));
            }

            return (inputCommand, cancellationToken) => 
                commandAsyncHandler.HandleAsync((TCommand)inputCommand ?? throw new ArgumentException("Invalid command.", nameof(inputCommand)), cancellationToken);
        }

        internal static MessageHandlerDelegate FromCommandHandler<TCommand>(ICommandHandler<TCommand> commandHandler)
            where TCommand : class
        {
            if (commandHandler == null)
            {
                throw new ArgumentNullException(nameof(commandHandler));
            }

            return (inputCommand, cancellationToken) =>
            {
                try
                {
                    commandHandler.Handle((TCommand)inputCommand ?? throw new ArgumentException("Invalid command.", nameof(inputCommand)));
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
        
        internal static Func<TCommand, CancellationToken, Task> FromCommandHandlerFactory<TCommand>(Func<ICommandAsyncHandler<TCommand>> commandHandlerFactory)
            where TCommand : class
        {
            if (commandHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(commandHandlerFactory));
            }

            return (inputCommand, cancellationToken) =>
            {
                if(!TryGetInstanceFromFactory(commandHandlerFactory, out ICommandAsyncHandler<TCommand> instance, out Exception exception))
                {
                    // Exception occurred or null is returned by factory.
                    return TaskUtility.FromException(exception);
                }
                
                return instance.HandleAsync((TCommand)inputCommand ?? throw new ArgumentException("Invalid command.", nameof(inputCommand)), cancellationToken);
            };
        }

        internal static Func<TCommand, CancellationToken, Task> FromCommandHandlerFactory<TCommand>(Func<ICommandHandler<TCommand>> commandHandlerFactory)
            where TCommand : class
        {
            if (commandHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(commandHandlerFactory));
            }

            return (inputCommand, ct) =>
            {
                try
                {                    
                    if(!TryGetInstanceFromFactory(commandHandlerFactory, out ICommandHandler<TCommand> instance, out Exception exception))
                    {
                        // Exception occurred or null is returned by factory.
                        return TaskUtility.FromException(exception);
                    }

                    instance.Handle((TCommand)inputCommand ?? throw new ArgumentException("Invalid command.", nameof(inputCommand)));
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
        
        internal static Func<TCommand, CancellationToken, Task> FromDelegate<TCommand>(Func<object> attributedObjectFactory, 
                                                                                       Func<object, TCommand, Task> asyncDelegate)
                                                                                       where TCommand : class
        {
            if (attributedObjectFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedObjectFactory));
            }

            if (asyncDelegate == null)
            {
                throw new ArgumentNullException(nameof(asyncDelegate));
            }

            return (inputCommand, ct) =>
            {
                if(!TryGetInstanceFromFactory(attributedObjectFactory, out var instance, out Exception exception))
                {
                    // Exception occurred or null is returned by factory.
                    return TaskUtility.FromException(exception);
                }
                
                return asyncDelegate.Invoke(instance, (TCommand)inputCommand ?? throw new ArgumentException("Invalid command.", nameof(inputCommand)));
            };
        }

        internal static Func<TCommand, CancellationToken, Task> FromDelegate<TCommand>(Func<object> attributedObjectFactory, 
                                                                                       Func<object, TCommand, CancellationToken, Task> cancellableAsyncDelegate)
                                                                                       where TCommand : class
        {
            if (attributedObjectFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedObjectFactory));
            }

            if (cancellableAsyncDelegate == null)
            {
                throw new ArgumentNullException(nameof(cancellableAsyncDelegate));
            }

            return (inputCommand, cancellationToken) =>
            {
                if (!TryGetInstanceFromFactory(attributedObjectFactory, out var instance, out Exception exception))
                {
                    // Exception occurred or null is returned by factory.
                    return TaskUtility.FromException(exception);
                }

                return cancellableAsyncDelegate.Invoke(instance, (TCommand)inputCommand ?? throw new ArgumentException("Invalid command.", nameof(inputCommand)), cancellationToken);
            };
        }

        internal static Func<TCommand, CancellationToken, Task> FromDelegate<TCommand>(Func<object> attributedObjectFactory, 
                                                                                       Action<object, TCommand> action)
                                                                                       where TCommand : class
        {
            if (attributedObjectFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedObjectFactory));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return (inputCommand, ct) =>
            {
                try
                {
                    if(!TryGetInstanceFromFactory(attributedObjectFactory, out var instance, out Exception exception))
                    {
                        // Exception occurred or null is returned by factory.
                        return TaskUtility.FromException(exception);
                    }

                    action.Invoke(instance, (TCommand)inputCommand ?? throw new ArgumentException("Invalid command.", nameof(inputCommand)));
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
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            
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
            return new InvalidOperationException($"Failed to retrieve an instance of {typeof(TInstance).Name} from the instance factory delegate. Please check registration configuration.", ex);
        }

        #endregion Functions
    }
}
