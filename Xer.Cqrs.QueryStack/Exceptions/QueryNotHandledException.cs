using System;

namespace Xer.Cqrs.QueryStack
{
    public class QueryNotHandledException : Exception
    {
        public QueryNotHandledException(string message) 
            : base(message)
        {
        }

        public QueryNotHandledException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
