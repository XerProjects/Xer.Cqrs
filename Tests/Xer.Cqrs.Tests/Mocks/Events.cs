using Xer.Cqrs.Events;

namespace Xer.Cqrs.Tests.Mocks
{
    public class TestEvent : IEvent
    {

    }

    public class TriggerLongRunningEvent : IEvent
    {
        public int Milliseconds { get; }

        public TriggerLongRunningEvent(int milliseconds)
        {
            Milliseconds = milliseconds;
        }
    }
}
