//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Xer.Worker.Async;

//namespace Xer.Worker.ExecutionStrategies
//{
//    public class DelayedExecutionStrategy : IWorkExecutionStrategy, IAsyncWorkExecutionStrategy
//    {
//        public TimeSpan DelayInMilliseconds { get; }

//        public DelayedExecutionStrategy(TimeSpan delayInMilliseconds)
//        {
//            if(delayInMilliseconds.TotalMilliseconds <= 0)
//            {
//                delayInMilliseconds = TimeSpan.FromMilliseconds(5);
//            }

//            DelayInMilliseconds = delayInMilliseconds;
//        }

//        public DelayedExecutionStrategy(int delayInMilliseconds)
//            : this(TimeSpan.FromMilliseconds(delayInMilliseconds))
//        {
//        }

//        public void Execute(IWork work)
//        {
//            if (DelayInMilliseconds.TotalMilliseconds > 0)
//            {
//                Task.Delay(DelayInMilliseconds).ConfigureAwait(false).GetAwaiter().GetResult();
//            }

//            work.Execute();
//        }

//        public async Task ExecuteAsync(IAsyncWork work, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            if (DelayInMilliseconds.TotalMilliseconds > 0)
//            {
//                await Task.Delay(DelayInMilliseconds, cancellationToken).ConfigureAwait(false);
//            }

//            await work.ExecuteAsync(cancellationToken).ConfigureAwait(false);
//        }
//    }
//}
