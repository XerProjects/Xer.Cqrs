namespace Xer.Cqrs.Events
{
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Make an operation based on the event.
        /// </summary>
        /// <param name="event">Event.</param>
        void Handle(TEvent @event);
    }
}
