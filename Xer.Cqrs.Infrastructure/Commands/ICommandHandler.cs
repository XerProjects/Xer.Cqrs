using Xer.Cqrs.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.Infrastructure.Commands
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        void Handle(TCommand command);
    }
}
