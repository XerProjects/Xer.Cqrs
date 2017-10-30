using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Xer.Cqrs.Events
{
    internal class TaskUtility
    {
        internal static readonly Task CompletedTask = Task.FromResult(true);

        internal static Task CreateFaultedTask(Exception ex)
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
            completionSource.TrySetException(ex);
            return completionSource.Task;
        }
    }
}
