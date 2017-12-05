using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.EventStack
{
    internal class EventHandlerDelegateBuilder
    {
        #region From EventHandler

        internal static EventHandlerDelegate FromEventHandler<TEvent>(IEventAsyncHandler<TEvent> eventAsyncHandler)
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate(async (inputEvent, ct) =>
            {
                TEvent @event = EnsureValidEvent<TEvent>(inputEvent);
                await eventAsyncHandler.HandleAsync(@event, ct).ConfigureAwait(false);
            });
        }

        internal static EventHandlerDelegate FromEventHandler<TEvent>(IEventHandler<TEvent> eventHandler)
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate(async (inputEvent, ct) =>
            {
                TEvent @event = EnsureValidEvent<TEvent>(inputEvent);

                // Yield so the sync handler will be scheduled to execute asynchronously.
                // This will allow other handlers to start execution.
                await Task.Yield();
                eventHandler.Handle(@event);
            });
        }

        #endregion From EventHandler

        #region From Factory

        internal static EventHandlerDelegate FromFactory<TEvent>(Func<IEventAsyncHandler<TEvent>> eventHandlerFactory)
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate(async (inputEvent, ct) =>
            {
                TEvent @event = EnsureValidEvent<TEvent>(inputEvent);
                IEventAsyncHandler<TEvent> instance = EnsureInstanceFromFactory(eventHandlerFactory);
                
                await instance.HandleAsync(@event, ct).ConfigureAwait(false);
            });
        }

        internal static EventHandlerDelegate FromFactory<TEvent>(Func<IEventHandler<TEvent>> eventHandlerFactory)
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate(async (inputEvent, ct) =>
            {
                TEvent @event = EnsureValidEvent<TEvent>(inputEvent);
                IEventHandler<TEvent> instance = EnsureInstanceFromFactory(eventHandlerFactory);

                // Yield so the sync handler will be scheduled to execute asynchronously.
                // This will allow other handlers to start execution.
                await Task.Yield();
                instance.Handle(@event);
            });
        }

        #endregion From Factory

        #region From Delegate

        internal static EventHandlerDelegate FromDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory, Func<TAttributed, TEvent, Task> asyncAction)
            where TAttributed : class
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate(async (inputEvent, ct) =>
            {
                TEvent @event = EnsureValidEvent<TEvent>(inputEvent);
                TAttributed instance = EnsureInstanceFromFactory(attributedObjectFactory);
                
                await asyncAction.Invoke(instance, @event).ConfigureAwait(false);
            });
        }

        internal static EventHandlerDelegate FromDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory, Func<TAttributed, TEvent, CancellationToken, Task> cancellableAsyncAction)
            where TAttributed : class
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate(async (inputEvent, ct) =>
            {
                TEvent @event = EnsureValidEvent<TEvent>(inputEvent);
                TAttributed instance = EnsureInstanceFromFactory(attributedObjectFactory);
                
                await cancellableAsyncAction.Invoke(instance, @event, ct).ConfigureAwait(false);
            });
        }

        internal static EventHandlerDelegate FromDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory, Action<TAttributed, TEvent> action)
            where TAttributed : class
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate(async (inputEvent, ct) =>
            {
                TEvent @event = EnsureValidEvent<TEvent>(inputEvent);
                TAttributed instance = EnsureInstanceFromFactory(attributedObjectFactory);

                // Yield so the sync handler will be scheduled to execute asynchronously.
                // This will allow other handlers to start execution.
                await Task.Yield();
                action.Invoke(instance, @event);
            });
        }

        #endregion From Delegate

        #region Functions

        private static TInstance EnsureInstanceFromFactory<TInstance>(Func<TInstance> factory)
        {
            try
            {
                TInstance instance = factory.Invoke();

                if (instance == null)
                {
                    throw ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<TInstance>();
                }

                return instance;
            }
            catch (Exception ex)
            {
                throw ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<TInstance>(ex);
            }
        }
        
        private static TEvent EnsureValidEvent<TEvent>(IEvent inputEvent) where TEvent : class
        {
            if (inputEvent == null)
            {
                throw new ArgumentNullException(nameof(inputEvent));
            }

            TEvent @event = inputEvent as TEvent;
            if (@event == null)
            {
                throw ExceptionBuilder.InvalidEventTypeArgumentException(typeof(TEvent), inputEvent.GetType());
            }

            return @event;
        }

        #endregion Functions
    }
}