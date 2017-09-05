using Xer.Cqrs.Validation;
using System;

namespace Xer.Cqrs.Commands
{
    public interface ICommand
    {
        Guid CommandId { get; }
    }
}
