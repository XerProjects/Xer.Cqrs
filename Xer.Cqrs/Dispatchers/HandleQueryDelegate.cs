using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xer.Cqrs.Dispatchers
{
    /// <summary>
    /// Delegate to handle queries.
    /// </summary>
    /// <param name="query">Query to handle.</param>
    /// <returns>Query result.</returns>
    internal delegate Task<object> HandleQueryDelegate(IQuery<object> query);
}
