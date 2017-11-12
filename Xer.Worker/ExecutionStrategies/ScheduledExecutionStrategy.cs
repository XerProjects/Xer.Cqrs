//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Xer.Worker.Async;

//namespace Xer.Worker.ExecutionStrategies
//{
//    public class ScheduledExecutionStrategy : IWorkExecutionStrategy, IAsyncWorkExecutionStrategy
//    {        
//        public DateTime ScheduledExecutionDate { get; }

//        public ScheduledExecutionStrategy(DateTime schedule)
//        {
//            ScheduledExecutionDate = schedule;
//        }

//        public void Execute(IWork work)
//        {
//            DateTime currentDate = DateTime.Now;

//            if (currentDate < ScheduledExecutionDate)
//            {
//                TimeSpan timeDifference = ScheduledExecutionDate - currentDate;

//                Task.Delay(timeDifference).ConfigureAwait(false).GetAwaiter().GetResult();

//                work.Execute();
//            }
//        }

//        public async Task ExecuteAsync(IAsyncWork work, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            DateTime currentDate = DateTime.Now;

//            if (currentDate < ScheduledExecutionDate)
//            {
//                TimeSpan timeDifference = ScheduledExecutionDate - currentDate;

//                await Task.Delay(timeDifference, cancellationToken).ConfigureAwait(false);

//                await work.ExecuteAsync(cancellationToken).ConfigureAwait(false);
//            }
//        }
//    }
//}
