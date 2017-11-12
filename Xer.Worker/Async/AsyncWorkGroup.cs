using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Worker.Async
{
    public class AsyncWorkGroup : IAsyncWork
    {
        private readonly List<IAsyncWork> _workList;

        public AsyncWorkGroup(IEnumerable<IAsyncWork> work)
        {
            _workList = new List<IAsyncWork>(work);
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            List<Task> executedTasks = new List<Task>(_workList.Count);

            foreach(IAsyncWork work in _workList)
            {
                Task executedTask = work.ExecuteAsync(cancellationToken);

                executedTasks.Add(executedTask);
            }

            while (executedTasks.Count > 0)
            {
                Task completedTask = await Task.WhenAny(executedTasks);
                executedTasks.Remove(completedTask);
                await completedTask;
            }
        }
    }
}
