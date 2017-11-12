using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Worker.Async
{
    public abstract class AsyncWorkerBase : IAsyncWorker
    {
        private readonly List<IAsyncWork> _workList = new List<IAsyncWork>();

        //public IAsyncWorkExecutionStrategy WorkExecutionStrategy { get; }

        public bool IsWorking { get; private set; }

        //public AsyncWorkerBase(IAsyncWorkExecutionStrategy executionStrategy)
        //{
        //    WorkExecutionStrategy = executionStrategy;
        //}

        //public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    OnStart();

        //    List<Task<IAsyncWork>> executingTasks = new List<Task<IAsyncWork>>(_workList.Count);

        //    // Start all work.
        //    foreach (IAsyncWork work in _workList)
        //    {
        //        Task<IAsyncWork> executedTask = ExecuteWorkAsync(work, cancellationToken);

        //        executingTasks.Add(executedTask);
        //    }

        //    while (!_done)
        //    {
        //        if(executingTasks.Count == 0)
        //        {
        //            break;
        //        }

        //        try
        //        {
        //            Task<IAsyncWork> completedTask = await Task.WhenAny(executingTasks);
        //            // Remove completed task work.
        //            executingTasks.Remove(completedTask);

        //            IAsyncWork completedWork = await completedTask;

        //            // Re-execute completed task.
        //            Task<IAsyncWork> executedTask = ExecuteWorkAsync(completedWork, cancellationToken);
        //            executingTasks.Add(executedTask);
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            _done = true;
        //        }
        //        catch (Exception ex)
        //        {
        //            if (!HandleError(ex))
        //            {
        //                throw;
        //            }
        //        }
        //    }
        //}

        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            OnStart();

            try
            {
                await StartWorkingAsync(_workList, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Cancelled.
            }
            catch (Exception ex)
            {
                if (!HandleError(ex))
                {
                    throw;
                }
            }
        }
        
        protected virtual Task StartWorkingAsync(IEnumerable<IAsyncWork> workList, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<Task<IAsyncWork>> executingTasks = new List<Task<IAsyncWork>>(_workList.Count);

            // Start all work.
            foreach (IAsyncWork work in _workList)
            {
                Task<IAsyncWork> executedTask = ExecuteWorkAsync(work, cancellationToken);

                executingTasks.Add(executedTask);
            }

            //Task<IAsyncWork> completedTask = await Task.WhenAny(executingTasks);
            //// Remove completed task work.
            //executingTasks.Remove(completedTask);

            //IAsyncWork completedWork = await completedTask;

            //// Re-execute completed task.
            //Task<IAsyncWork> executedTask = ExecuteWorkAsync(completedWork, cancellationToken);
            //executingTasks.Add(executedTask);

            return Task.WhenAll(executingTasks);
        }
        
        protected virtual async Task<IAsyncWork> ExecuteWorkAsync(IAsyncWork work, CancellationToken cancellationToken = default(CancellationToken))
        {
            //await WorkExecutionStrategy.ExecuteAsync(work, cancellationToken).ConfigureAwait(false);

            await work.ExecuteAsync(cancellationToken);

            return work;
        }

        public void AssignWork(IAsyncWork work)
        {
            _workList.Add(work);
        }
        
        public void Stop()
        {
            OnStop();
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
