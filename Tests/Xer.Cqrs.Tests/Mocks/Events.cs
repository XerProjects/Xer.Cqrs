using Xer.Cqrs.EventStack;

namespace Xer.Cqrs.Tests.Mocks
{
    public class TestEvent1 : IEvent
    {

    }

    public class TestEvent2 : IEvent
    {

    }

    public class TestEvent3 : IEvent
    {

    }

    public class TriggerLongRunningEvent : IEvent
    {
        public int DurationInMilliseconds { get; }

        public TriggerLongRunningEvent(int durationInMilliseconds)
        {
            DurationInMilliseconds = durationInMilliseconds;
        }
    }
}
