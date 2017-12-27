using System;

namespace Xer.Cqrs.CommandStack
{
    public interface ICommand
    {
        Guid CommandId { get; }
    }
}
