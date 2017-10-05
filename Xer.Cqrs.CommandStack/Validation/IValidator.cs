using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.CommandStack.Validation
{
    public interface IValidator<in TTarget> where TTarget : class
    {
        void Validate(TTarget target);
    }
}
