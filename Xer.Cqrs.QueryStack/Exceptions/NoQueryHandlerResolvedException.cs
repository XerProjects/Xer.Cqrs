using System;

namespace Xer.Cqrs.QueryStack
{
    public class NoQueryHandlerResolvedException : Exception
    {
        public NoQueryHandlerResolvedException(string message) 
            : base(message)
        {
        }

        public NoQueryHandlerResolvedException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
