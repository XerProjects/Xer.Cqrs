namespace Xer.Cqrs.EventStack
{
    public interface IEventHandler<TEvent> where TEvent : class
    {
        /// <summary>
        /// Handle event synchronously.
        /// </summary>
        /// <param name="event">Event to handle.</param>
        void Handle(TEvent @event);
    }
}
