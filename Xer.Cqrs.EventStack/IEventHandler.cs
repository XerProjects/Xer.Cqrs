namespace Xer.Cqrs.EventStack
{
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Make an operation based on the event.
        /// </summary>
        /// <param name="event">Event to handle.</param>
        void Handle(TEvent @event);
    }
}
