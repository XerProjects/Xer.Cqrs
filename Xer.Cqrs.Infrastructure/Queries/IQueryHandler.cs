using Xer.Cqrs.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.Infrastructure.Queries
{
    public interface IQueryHandler<in TQuery, out TResult> where TQuery : IQuery<TResult>
    {
        TResult Handle(TQuery query);
    }
}
