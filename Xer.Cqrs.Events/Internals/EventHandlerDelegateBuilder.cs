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
            return new EventHandlerDelegate((c, ct) =>
            {
                TEvent @event = c as TEvent;
                if (@event == null)
                {
                    throw ExceptionBuilder.InvalidEventTypeArgumentException(typeof(TEvent), c.GetType());
                }

                try
                {
                    eventHandler.Handle(@event);
                    return TaskUtility.CompletedTask;
                }
                catch (Exception ex)
                {
                    return TaskUtility.CreateFaultedTask(ex);
                }
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

                IEventAsyncHandler<TEvent> instance;

                if (!TryRetrieveInstanceFromFactory(eventHandlerFactory, out instance))
                {
                    throw ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<IEventAsyncHandler<TEvent>>();
                }

                await instance.HandleAsync(@event, ct).ConfigureAwait(false);
            });
        }

        internal static EventHandlerDelegate FromFactory<TEvent>(Func<IEventHandler<TEvent>> eventHandlerFactory)
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate((e, ct) =>
            {
                TEvent @event = e as TEvent;
                if (@event == null)
                {
                    throw ExceptionBuilder.InvalidEventTypeArgumentException(typeof(TEvent), e.GetType());
                }

                IEventHandler<TEvent> instance;

                if (!TryRetrieveInstanceFromFactory(eventHandlerFactory, out instance))
                {
                    return TaskUtility.CreateFaultedTask(ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<IEventHandler<TEvent>>());
                }

                try
                {
                    instance.Handle(@event);
                    return TaskUtility.CompletedTask;
                }
                catch (Exception ex)
                {
                    return TaskUtility.CreateFaultedTask(ex);
                }
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

                TAttributed instance;

                if (!TryRetrieveInstanceFromFactory(attributedObjectFactory, out instance))
                {
                    throw ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<TAttributed>();
                }

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

                TAttributed instance;

                if (!TryRetrieveInstanceFromFactory(attributedObjectFactory, out instance))
                {
                    throw ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<TAttributed>();
                }

                await cancellableAsyncAction.Invoke(instance, @event, ct).ConfigureAwait(false);
            });
        }

        internal static EventHandlerDelegate FromDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory, Action<TAttributed, TEvent> action)
            where TAttributed : class
            where TEvent : class, IEvent
        {
            return new EventHandlerDelegate((e, ct) =>
            {
                TEvent @event = e as TEvent;
                if (@event == null)
                {
                    throw ExceptionBuilder.InvalidEventTypeArgumentException(typeof(TEvent), e.GetType());
                }

                TAttributed instance;

                if (!TryRetrieveInstanceFromFactory(attributedObjectFactory, out instance))
                {
                    return TaskUtility.CreateFaultedTask(ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<TAttributed>());
                }

                try
                {
                    action.Invoke(instance, @event);
                    return TaskUtility.CompletedTask;
                }
                catch (Exception ex)
                {
                    return TaskUtility.CreateFaultedTask(ex);
                }
            });
        }

        #endregion From Delegate

        #region Functions

        private static bool TryRetrieveInstanceFromFactory<TInstance>(Func<TInstance> factory, out TInstance instance)
        {
            instance = default(TInstance);

            try
            {
                instance = factory.Invoke();

                if (instance == null)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        #endregion Functions
    }
}
