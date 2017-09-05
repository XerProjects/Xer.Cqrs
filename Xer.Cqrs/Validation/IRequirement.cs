using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.Validation
{
    public interface IRequirement<TTarget>
    {
        string ErorMessage { get; }
        bool IsSatisfiedBy(TTarget target);
    }
}
