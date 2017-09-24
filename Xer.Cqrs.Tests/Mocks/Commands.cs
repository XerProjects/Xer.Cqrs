using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.Tests.Mocks
{
    public class DoSomethingCommand : Command
    {

    }

    public class DoSomethingAsyncCommand : Command
    {

    }

    public class DoSomethingAsyncWithCancellationCommand : Command
    {

    }

    public class DoSomethingAsyncForSpecifiedDurationCommand : Command
    {
        public int DurationInMilliSeconds { get; }

        public DoSomethingAsyncForSpecifiedDurationCommand(int milliSeconds)
        {
            DurationInMilliSeconds = milliSeconds;
        }
    }
}
