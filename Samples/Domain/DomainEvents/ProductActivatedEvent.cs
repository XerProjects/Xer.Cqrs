namespace Domain.DomainEvents
{
    public class ProductActivatedEvent : IDomainEvent
    {
        public int ProductId { get; }

        public ProductActivatedEvent(int productId)
        {
            ProductId = productId;
        }
    }
}