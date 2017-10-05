using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.CommandStack
{
    public class CommandNotHandledException : Exception
    {
        public CommandNotHandledException(string message) 
            : base(message)
        {
        }

        public CommandNotHandledException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
