//using System.Threading;
//using System.Threading.Tasks;
//using Xer.Worker.Async;

//namespace Xer.Worker.ExecutionStrategies
//{
//    public class DefaultExecutionStrategy : IWorkExecutionStrategy, IAsyncWorkExecutionStrategy
//    {
//        public void Execute(IWork work)
//        {
//            work.Execute();
//        }

//        public Task ExecuteAsync(IAsyncWork work, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            return work.ExecuteAsync(cancellationToken);
//        }
//    }
//}
