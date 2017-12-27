using System;

namespace Xer.Cqrs.CommandStack
{
    public abstract class Command : ICommand
    {
        public Guid CommandId { get; } = Guid.NewGuid();
    }
}
