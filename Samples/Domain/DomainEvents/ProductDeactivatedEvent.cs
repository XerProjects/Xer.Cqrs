namespace Domain.DomainEvents
{
    public class ProductDeactivatedEvent : IDomainEvent
    {
        public int ProductId { get; }

        public ProductDeactivatedEvent(int productId)
        {
            ProductId = productId;
        }
    }
}