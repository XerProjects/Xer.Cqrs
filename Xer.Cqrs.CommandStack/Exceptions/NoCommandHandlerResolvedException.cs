using System;

namespace Xer.Cqrs.CommandStack
{
    public class NoCommandHandlerResolvedException : Exception
    {
        public NoCommandHandlerResolvedException(string message) 
            : base(message)
        {
        }

        public NoCommandHandlerResolvedException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}