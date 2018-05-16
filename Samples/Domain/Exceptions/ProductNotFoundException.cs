using System;
using Xer.DomainDriven.Exceptions;

namespace Domain.Exceptions
{
    public class ProductNotFoundException : AggregateRootNotFoundException
    {
        public ProductNotFoundException(string message) : base(message)
        {
        }

        public ProductNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}