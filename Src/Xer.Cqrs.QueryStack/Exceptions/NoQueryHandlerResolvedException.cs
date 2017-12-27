using System;

namespace Xer.Cqrs.QueryStack
{
    public class NoQueryHandlerResolvedException : Exception
    {
        public Type QueryType { get; }

        public NoQueryHandlerResolvedException(string message, Type queryType) 
            : base(message)
        { 
        }

        public NoQueryHandlerResolvedException(string message, Type queryType, Exception innerException) 
            : base(message, innerException)
        {
            QueryType = queryType;
        }
    }
}