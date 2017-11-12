using System;
using System.Threading.Tasks;

namespace Xer.Worker
{
    public class BackgroundThreadWorker : IWorker
    {
        private readonly IWorker _worker;

        //public IWorkExecutionStrategy WorkExecutionStrategy => _worker.WorkExecutionStrategy;

        public bool IsWorking => _worker.IsWorking;

        public BackgroundThreadWorker(IWorker worker)
        {
            _worker = worker ?? throw new ArgumentNullException(nameof(worker));
        }
        
        public void AssignWork(IWork work)
        {
            _worker.AssignWork(work);
        }

        public void Start()
        {
            // Run on separate thread.
            Task.Factory.StartNew(() => _worker.Start(), TaskCreationOptions.LongRunning)
            .ContinueWith(async t =>
            {
                // Await so that exceptions that may have occured will be propagated.
                await t;
            });
        }

        public void Stop()
        {
            _worker.Stop();
        }
    }
}
