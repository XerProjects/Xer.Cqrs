using Xer.Cqrs.EventStack;

namespace DomainEvents
{
    public class ProductRegisteredEvent : IEvent
    {
        public int ProductId { get; }
        public string ProductName { get; }

        public ProductRegisteredEvent(int productId, string productName)
        {
            ProductId = productId;
            ProductName = productName;
        }
    }
}