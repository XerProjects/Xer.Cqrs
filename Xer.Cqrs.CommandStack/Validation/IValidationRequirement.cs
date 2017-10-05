using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.CommandStack.Validation
{
    public interface IValidationRequirement<TTarget>
    {
        string ErrorMessage { get; }
        bool IsSatisfiedBy(TTarget target);
    }
}
