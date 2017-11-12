using System;
using System.Runtime.Serialization;

namespace Xer.Messajing.Resolvers
{
    public class MessageHandlerNotRegisteredException : Exception
    {
        public MessageHandlerNotRegisteredException()
        {
        }

        public MessageHandlerNotRegisteredException(string message) 
            : base(message)
        {
        }

        public MessageHandlerNotRegisteredException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}