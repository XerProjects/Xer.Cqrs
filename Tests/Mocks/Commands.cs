using Xer.Cqrs.CommandStack;

namespace Xer.Cqrs.Tests.Mocks
{
    public class DoSomethingCommand
    {

    }

    public class ThrowExceptionCommand
    {

    }

    public class DoSomethingWithCancellationCommand
    {

    }

    public class DoSomethingForSpecifiedDurationCommand
    {
        public int DurationInMilliSeconds { get; }

        public DoSomethingForSpecifiedDurationCommand(int milliSeconds)
        {
            DurationInMilliSeconds = milliSeconds;
        }
    }
}
