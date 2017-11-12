using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xer.Worker
{
    public class IntervalWorker : IWorker
    {
        private bool _done = false;
        private readonly IWorker _worker;

        //public IWorkExecutionStrategy WorkExecutionStrategy => _worker.WorkExecutionStrategy;

        public int IntervalInMillisecods { get; }

        public bool IsWorking => _worker.IsWorking;

        public IntervalWorker(IWorker worker, int intervalInMillisecods) 
        {
            _worker = worker;
            IntervalInMillisecods = intervalInMillisecods;
        }

        public void AssignWork(IWork work)
        {
            _worker.AssignWork(work);
        }

        public void Start()
        {
            bool firstRun = true;

            while (!_done)
            {
                // Do not delay on first run.
                if (!firstRun)
                {
                    Task.Delay(IntervalInMillisecods).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                _worker.Start();

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
