using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xer.Worker
{
    public abstract class WorkerBase : IWorker
    {
        private readonly List<IWork> _workList = new List<IWork>();

        //public IWorkExecutionStrategy WorkExecutionStrategy { get; }

        public bool IsWorking { get; private set; }

        //public WorkerBase(IWorkExecutionStrategy executionStrategy)
        //{
        //    WorkExecutionStrategy = executionStrategy;
        //}

        public void AssignWork(IWork work)
        {
            _workList.Add(work);
        }

        public void Start()
        {
            OnStart();

            try
            {
                StartWorking(_workList);
            }
            catch (Exception ex)
            {
                if (!HandleError(ex))
                {
                    throw;
                }
            }
        }

        protected virtual void StartWorking(IEnumerable<IWork> workList)
        {
            IsWorking = true;

            foreach (IWork work in workList)
            {
                ExecuteWork(work);
            }
        }

        protected virtual void ExecuteWork(IWork work)
        {
            work.Execute();
        }

        public void Stop()
        {
            OnStop();
            StopWorking();

            IsWorking = false;
        }

        protected virtual void StopWorking()
        {
        }

        protected virtual void OnStart()
        {

        }

        protected virtual void OnStop()
        {

        }

        protected virtual bool HandleError(Exception ex)
        {
            return false;
        }
    }
}
