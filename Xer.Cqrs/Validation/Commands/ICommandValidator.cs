using Xer.Cqrs.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.Validation.Commands
{
    public interface ICommandValidator : IValidator<ICommand>
    {
    }
}
