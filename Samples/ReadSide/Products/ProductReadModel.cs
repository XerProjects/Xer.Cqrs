using System;

namespace ReadSide.Products
{
    public class ProductReadModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public bool IsActive { get; set; }
    }
}