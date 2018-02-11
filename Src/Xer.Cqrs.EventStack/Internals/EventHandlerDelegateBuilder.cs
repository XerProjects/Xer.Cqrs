using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xer.Delegator;

namespace Xer.Cqrs.EventStack
{
    internal class EventHandlerDelegateBuilder
    {
        #region From EventHandler

        internal static MessageHandlerDelegate FromEventHandler<TEvent>(IEventAsyncHandler<TEvent> eventAsyncHandler)
            where TEvent : class
        {
            if (eventAsyncHandler == null)
            {
                throw new ArgumentNullException(nameof(eventAsyncHandler));
            }

            return (inputEvent, ct) => eventAsyncHandler.HandleAsync((TEvent)inputEvent, ct);
        }

        internal static MessageHandlerDelegate FromEventHandlers<TEvent>(IEnumerable<IEventAsyncHandler<TEvent>> eventHandlers)
            where TEvent : class
        {
            if (eventHandlers == null)
            {
                throw new ArgumentNullException(nameof(eventHandlers));
            }
            
            // Capture.
            List<IEventAsyncHandler<TEvent>> handlerList = eventHandlers.ToList();

            return (message, cancellationToken) =>
            {
                // Task list.
                Task[] handleTasks = new Task[handlerList.Count];

                // Invoke each message handler delegates to start the tasks and add to task list.
                for (int i = 0; i < handlerList.Count; i++)
                    handleTasks[i] = handlerList[i].HandleAsync((TEvent)message, cancellationToken);

                // Wait for all tasks to complete.
                return Task.WhenAll(handleTasks);
            };
        }

        internal static MessageHandlerDelegate FromEventHandler<TEvent>(IEventHandler<TEvent> eventHandler, bool yieldExecution = false)
            where TEvent : class
        {
            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            if (yieldExecution)
            {
                // Return a message handler delegate that will yield execution.
                return async (inputEvent, ct) =>
                {
                    // Yield so the sync handler will be scheduled to execute asynchronously.
                    // This will allow other handlers to start execution.
                    await Task.Yield();

                    eventHandler.Handle((TEvent)inputEvent);
                };
            }
            
            // Return a message handler delegate that does not yield execution.
            return (inputEvent, ct) =>
            {
                try
                {
                    eventHandler.Handle((TEvent)inputEvent);
                    return TaskUtility.CompletedTask;
                }
                catch(Exception ex)
                {
                    return TaskUtility.FromException(ex);
                }
            };
        }

        internal static MessageHandlerDelegate FromEventHandlers<TEvent>(IEnumerable<IEventHandler<TEvent>> eventHandlers, 
                                                                         bool yieldExecution = false)
                                                                         where TEvent : class
        {
            if (eventHandlers == null)
            {
                throw new ArgumentNullException(nameof(eventHandlers));
            }
            
            // Capture.
            List<IEventHandler<TEvent>> handlerList = eventHandlers.ToList();

            if (yieldExecution)
            {
                // Return a message handler delegate that will yield execution.
                return async (inputEvent, ct) =>
                {
                    // Yield so the sync handler will be scheduled to execute asynchronously.
                    // This will allow other handlers to start execution.
                    await Task.Yield();

                    for (int i = 0; i < handlerList.Count; i++)
                    {
                        handlerList[i].Handle((TEvent)inputEvent);
                    }
                };
            }

            // Return a message handler delegate that does not yield execution.
            return (inputEvent, ct) =>
            {
                try
                {
                    for (int i = 0; i < handlerList.Count; i++)
                    {
                        handlerList[i].Handle((TEvent)inputEvent);
                    }

                    return TaskUtility.CompletedTask;
                }
                catch(Exception ex)
                {
                    return TaskUtility.FromException(ex);
                }
            };
        }

        #endregion From EventHandler

        #region From EventHandlerFactory

        internal static Func<TEvent, CancellationToken, Task> FromEventHandlerFactory<TEvent>(Func<IEventAsyncHandler<TEvent>> eventHandlerFactory)
            where TEvent : class
        {
            if (eventHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerFactory));
            }

            return (inputEvent, ct) =>
            {
                if (!TryGetInstanceFromFactory(eventHandlerFactory, out IEventAsyncHandler<TEvent> instance, out Exception exception))
                {
                    // Exception occurred or null is returned by factory.
                    return TaskUtility.FromException(exception);
                }

                return instance.HandleAsync(inputEvent, ct);
            };
        }

        internal static Func<TEvent, CancellationToken, Task> FromEventHandlerFactory<TEvent>(Func<IEventHandler<TEvent>> eventHandlerFactory, 
                                                                                              bool yieldExecution = false)
                                                                                              where TEvent : class
        {
            if (eventHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerFactory));
            }

            if (yieldExecution)
            {
                return async (inputEvent, ct) =>
                {
                    // Yield so the sync handler will be scheduled to execute asynchronously.
                    // This will allow other handlers to start execution.
                    await Task.Yield();

                    if (!TryGetInstanceFromFactory(eventHandlerFactory, out IEventHandler<TEvent> instance, out Exception exception))
                    {
                        // Exception occurred or null is returned by factory.
                        throw exception;
                    }

                    instance.Handle(inputEvent);
                };
            }

            return (inputEvent, ct) =>
            {
                try
                {
                    if (!TryGetInstanceFromFactory(eventHandlerFactory, out IEventHandler<TEvent> instance, out Exception exception))
                    {
                        // Exception occurred or null is returned by factory.
                        return TaskUtility.FromException(exception);
                    }

                    instance.Handle(inputEvent);
                    return TaskUtility.CompletedTask;
                }
                catch(Exception ex)
                {
                    return TaskUtility.FromException(ex);
                }
            };
        }

        #endregion From EventHandlerFactory

        #region From Delegate

        internal static Func<TEvent, CancellationToken, Task> FromDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory, 
                                                                                                Func<TAttributed, TEvent, Task> asyncAction)
                                                                                                where TAttributed : class
                                                                                                where TEvent : class
        {
            if (attributedObjectFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedObjectFactory));
            }

            if (asyncAction == null)
            {
                throw new ArgumentNullException(nameof(asyncAction));
            }

            return (inputEvent, ct) =>
            {
                if (!TryGetInstanceFromFactory(attributedObjectFactory, out TAttributed instance, out Exception exception))
                {
                    // Exception occurred or null is returned by factory.
                    return TaskUtility.FromException(exception);
                }

                return asyncAction.Invoke(instance, inputEvent);
            };
        }

        internal static Func<TEvent, CancellationToken, Task> FromDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory, 
                                                                                                Func<TAttributed, TEvent, CancellationToken, Task> cancellableAsyncAction)
                                                                                                where TAttributed : class
                                                                                                where TEvent : class
        {
            if (attributedObjectFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedObjectFactory));
            }

            if (cancellableAsyncAction == null)
            {
                throw new ArgumentNullException(nameof(cancellableAsyncAction));
            }

            return (inputEvent, ct) =>
            {
                if (!TryGetInstanceFromFactory(attributedObjectFactory, out TAttributed instance, out Exception exception))
                {
                    // Exception occurred or null is returned by factory.
                    return TaskUtility.FromException(exception);
                }
                
                return cancellableAsyncAction.Invoke(instance, inputEvent, ct);
            };
        }

        internal static Func<TEvent, CancellationToken, Task> FromDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory, 
                                                                                                Action<TAttributed, TEvent> action, 
                                                                                                bool yieldExecution = false)
                                                                                                where TAttributed : class
                                                                                                where TEvent : class
        {
            if (attributedObjectFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedObjectFactory));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (yieldExecution)
            {
                return async (inputEvent, ct) =>
                {
                    // Yield so the sync handler will be scheduled to execute asynchronously.
                    // This will allow other handlers to start execution.
                    await Task.Yield();

                    if (!TryGetInstanceFromFactory(attributedObjectFactory, out TAttributed instance, out Exception exception))
                    {
                        // Exception occurred or null is returned by factory.
                        throw exception;
                    }

                    action.Invoke(instance, inputEvent);
                };
            }

            return (inputEvent, ct) =>
            {
                try
                {
                    if (!TryGetInstanceFromFactory(attributedObjectFactory, out TAttributed instance, out Exception exception))
                    {
                        // Exception occurred or null is returned by factory.
                        return TaskUtility.FromException(exception);
                    }

                    action.Invoke(instance, inputEvent);
                    return TaskUtility.CompletedTask;
                }
                catch(Exception ex)
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
            // Locals.
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

        private static InvalidOperationException FailedToRetrieveInstanceFromFactoryDelegateException<T>(Exception innerException = null)
        {
            return new InvalidOperationException($"Failed to retrieve an event handler instance from the registered factory for {typeof(T).Name}.", innerException);
        }

        #endregion Functions
    }
}