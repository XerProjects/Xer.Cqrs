using System;

namespace Xer.Cqrs.CommandStack
{
    public class NoCommandHandlerResolvedException : Exception
    {
        public Type CommandType { get; }

        public NoCommandHandlerResolvedException(string message, Type commandType) 
            : this(message, commandType, null)
        {
        }

        public NoCommandHandlerResolvedException(string message, Type commandType, Exception innerException) 
            : base(message, innerException)
        { 
            CommandType = commandType;
        }
    }
}