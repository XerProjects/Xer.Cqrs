using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xer.Cqrs.Dispatchers
{
    /// <summary>
    /// Delatate to handle command.
    /// </summary>
    /// <param name="command">Command to handle.</param>
    internal delegate Task HandleCommandDelegate(ICommand command);
}
