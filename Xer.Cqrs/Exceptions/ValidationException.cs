using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Xer.Cqrs.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException()
        {
        }

        public ValidationException(string message) 
            : base(message)
        {
        }

        public ValidationException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected ValidationException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
