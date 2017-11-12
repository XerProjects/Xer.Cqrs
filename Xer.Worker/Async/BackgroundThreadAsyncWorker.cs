using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Worker.Async
{
    public class BackgroundThreadAsyncWorker : IAsyncWorker
    {
        private readonly IAsyncWorker _worker;

        public BackgroundThreadAsyncWorker(IAsyncWorker worker)
        {
            _worker = worker ?? throw new ArgumentNullException(nameof(worker));
        }

        //public IAsyncWorkExecutionStrategy WorkExecutionStrategy => _worker.WorkExecutionStrategy;

        public bool IsWorking => _worker.IsWorking;

        public void AssignWork(IAsyncWork work)
        {
            _worker.AssignWork(work);
        }

        public Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Run on separate thread.
            Task task = Task.Factory.StartNew(() => _worker.StartAsync(cancellationToken), TaskCreationOptions.LongRunning);

            task.ConfigureAwait(false);

            return task;
        }

        public void Stop()
        {
            _worker.Stop();
        }
    }
}
