using System;

namespace Xer.Cqrs
{
    public interface ICommand
    {
        Guid CommandId { get; }
    }
}
