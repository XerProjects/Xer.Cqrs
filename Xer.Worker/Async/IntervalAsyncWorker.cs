using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Worker.Async
{
    public class IntervalAsyncWorker : IAsyncWorker
    {
        private bool _done = false;

        private readonly IAsyncWorker _worker;

        public bool IsWorking => _worker.IsWorking;

        public int DelayInMilliseconds { get; }

        //public IAsyncWorkExecutionStrategy WorkExecutionStrategy => _worker.WorkExecutionStrategy;

        public IntervalAsyncWorker(IAsyncWorker worker, int delayInMilliseconds)
        {
            _worker = worker;
            DelayInMilliseconds = delayInMilliseconds;
        }

        public void AssignWork(IAsyncWork work)
        {
            _worker.AssignWork(work);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            bool firstRun = true;

            while(!_done)
            {
                // Do not delay on first run.
                if (!firstRun)
                {
                    await Task.Delay(DelayInMilliseconds, cancellationToken).ConfigureAwait(false);
                }

                await _worker.StartAsync(cancellationToken).ConfigureAwait(false);

                // Set after first run so that next run will be delayed.
                firstRun = false;
            }
        }

        public void Stop()
        {
            _done = true;
            _worker.Stop();
        }
    }
}
