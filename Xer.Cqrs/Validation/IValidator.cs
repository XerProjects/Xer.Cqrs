using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.Validation
{
    public interface IValidator<T>
    {
        void Validate(T obj);
        void AddRequirement<TTarget>(IRequirement<TTarget> requirement);
    }
}
