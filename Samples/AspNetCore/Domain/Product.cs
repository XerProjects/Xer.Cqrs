using System;

namespace AspNetCore.Domain
{
    public class Product
    {
        public int Id { get; }
        public string Name { get; }
        public bool IsActive { get; private set; }

        public Product(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}