using System.Threading;
using System.Threading.Tasks;
using DomainEvents;
using Xer.Cqrs.EventStack;

namespace Console.EventHandlers
{
    public class ProductRegisteredEventHandler : IEventHandler<ProductRegisteredEvent>
    {
        public void Handle(ProductRegisteredEvent @event)
        {
            System.Console.WriteLine($"{GetType().Name} handled {@event.GetType()}.");
        }
    }
}