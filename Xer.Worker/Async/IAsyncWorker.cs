using System.Threading;
using System.Threading.Tasks;

namespace Xer.Worker.Async
{
    public interface IAsyncWorker
    {
        /// <summary>
        /// Indicator if worker is currently executing work.
        /// </summary>
        bool IsWorking { get; }

        /// <summary>
        /// Work execution strategy.
        /// </summary>
        //IAsyncWorkExecutionStrategy WorkExecutionStrategy { get; }

        /// <summary>
        /// Register a work to be executed when this worker executes.
        /// </summary>
        /// <param name="work">Work which will be executed when allowed.</param>
        void AssignWork(IAsyncWork work);

        /// <summary>
        /// Start working.
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Stop working.
        /// </summary>
        void Stop();
    }
}
