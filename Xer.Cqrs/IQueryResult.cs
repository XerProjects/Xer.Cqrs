using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs
{
    public interface IQueryResult<out TResult>
    {
        TResult Result { get; }
    }
}
