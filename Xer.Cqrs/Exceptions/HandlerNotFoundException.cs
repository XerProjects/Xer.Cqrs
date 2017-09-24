using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs
{
    public class HandlerNotFoundException : Exception
    {
        public HandlerNotFoundException(string message) 
            : base(message)
        {
        }

        public HandlerNotFoundException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
