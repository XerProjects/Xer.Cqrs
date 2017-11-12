using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.Events
{
    internal class EventHandlerDelegateBuilder
    {
        #region From EventHandler

        internal static EventHandlerDelegate FromEventHandler<TEvent>(IEventAsyncHandler<TEvent> eventAsyncHandler)
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate(async (e, ct) =>
            {
                TEvent @event = e as TEvent;
                if (@event == null)
                {
                    throw ExceptionBuilder.InvalidEventTypeArgumentException(typeof(TEvent), e.GetType());
                }

                await eventAsyncHandler.HandleAsync(@event, ct).ConfigureAwait(false);
            });
        }

        internal static EventHandlerDelegate FromEventHandler<TEvent>(IEventHandler<TEvent> eventHandler)
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate(async (c, ct) =>
            {
                TEvent @event = c as TEvent;
                if (@event == null)
                {
                    throw ExceptionBuilder.InvalidEventTypeArgumentException(typeof(TEvent), c.GetType());
                }

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
            return new EventHandlerDelegate(async (e, ct) =>
            {
                TEvent @event = e as TEvent;
                if (@event == null)
                {
                    throw ExceptionBuilder.InvalidEventTypeArgumentException(typeof(TEvent), e.GetType());
                }

                IEventAsyncHandler<TEvent> instance = EnsureInstanceFromFactory(eventHandlerFactory);
                
                await instance.HandleAsync(@event, ct).ConfigureAwait(false);
            });
        }

        internal static EventHandlerDelegate FromFactory<TEvent>(Func<IEventHandler<TEvent>> eventHandlerFactory)
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate(async (e, ct) =>
            {
                TEvent @event = e as TEvent;
                if (@event == null)
                {
                    throw ExceptionBuilder.InvalidEventTypeArgumentException(typeof(TEvent), e.GetType());
                }

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
            return new EventHandlerDelegate(async (e, ct) =>
            {
                TEvent @event = e as TEvent;
                if (@event == null)
                {
                    throw ExceptionBuilder.InvalidEventTypeArgumentException(typeof(TEvent), e.GetType());
                }

                TAttributed instance = EnsureInstanceFromFactory(attributedObjectFactory);
                
                await asyncAction.Invoke(instance, @event).ConfigureAwait(false);
            });
        }

        internal static EventHandlerDelegate FromDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory, Func<TAttributed, TEvent, CancellationToken, Task> cancellableAsyncAction)
            where TAttributed : class
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate(async (e, ct) =>
            {
                TEvent @event = e as TEvent;
                if (@event == null)
                {
                    throw ExceptionBuilder.InvalidEventTypeArgumentException(typeof(TEvent), e.GetType());
                }

                TAttributed instance = EnsureInstanceFromFactory(attributedObjectFactory);
                
                await cancellableAsyncAction.Invoke(instance, @event, ct).ConfigureAwait(false);
            });
        }

        internal static EventHandlerDelegate FromDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory, Action<TAttributed, TEvent> action)
            where TAttributed : class
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate(async (e, ct) =>
            {
                TEvent @event = e as TEvent;
                if (@event == null)
                {
                    throw ExceptionBuilder.InvalidEventTypeArgumentException(typeof(TEvent), e.GetType());
                }

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

        #endregion Functions
    }
}