//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Xer.Worker
//{
//    public class ParallelWorker : WorkerBase
//    {
//        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

//        //public ParallelWorker(IWorkExecutionStrategy executionStrategy) 
//        //    : base(executionStrategy)
//        //{
//        //}

//        protected override void StartWorking(IEnumerable<IWork> workList)
//        {
//            ParallelOptions options = new ParallelOptions()
//            {
//                CancellationToken = _cancellationTokenSource.Token
//            };

//            try
//            {
//                Parallel.ForEach(workList, options, (w) => ExecuteWork(w));
//            }
//            catch(OperationCanceledException)
//            {
//                // Cancelled.
//            }
//        }

//        protected override void StopWorking()
//        {
//            _cancellationTokenSource.Cancel();

//            base.StopWorking();
//        }
//    }
//}
