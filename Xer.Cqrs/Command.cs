using System;

namespace Xer.Cqrs
{
    public abstract class Command : ICommand
    {
        public Guid CommandId { get; } = Guid.NewGuid();
    }
}
