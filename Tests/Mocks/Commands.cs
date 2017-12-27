using Xer.Cqrs.CommandStack;

namespace Xer.Cqrs.Tests.Mocks
{
    public class DoSomethingCommand : Command
    {

    }

    public class ThrowExceptionCommand : Command
    {

    }

    public class DoSomethingWithCancellationCommand : Command
    {

    }

    public class DoSomethingForSpecifiedDurationCommand : Command
    {
        public int DurationInMilliSeconds { get; }

        public DoSomethingForSpecifiedDurationCommand(int milliSeconds)
        {
            DurationInMilliSeconds = milliSeconds;
        }
    }
}
