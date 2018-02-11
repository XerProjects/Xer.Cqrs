using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class ProductRegisteredEvent : IDomainEvent
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