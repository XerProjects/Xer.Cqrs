using System;
using System.Collections.Generic;
using System.Text;
using Xer.Cqrs.Validation;
using Xer.Cqrs.Exceptions;

namespace Xer.Cqrs.Commands
{
    public abstract class Command : ICommand
    {
        public Guid CommandId { get; } = Guid.NewGuid();
    }
}
