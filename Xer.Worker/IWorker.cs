namespace Xer.Worker
{
    public interface IWorker
    {
        /// <summary>
        /// Indicator if worker is currently executing work.
        /// </summary>
        bool IsWorking { get; }

        /// <summary>
        /// Work execution strategy.
        /// </summary>
        //IWorkExecutionStrategy WorkExecutionStrategy { get; }

        /// <summary>
        /// Register a work to be executed when this worker executes.
        /// </summary>
        /// <param name="work">Work which will be executed when allowed.</param>
        void AssignWork(IWork work);

        /// <summary>
        /// Start working.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop working.
        /// </summary>
        void Stop();
    }
}
