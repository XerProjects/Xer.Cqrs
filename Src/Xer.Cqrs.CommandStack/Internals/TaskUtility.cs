namespace System.Threading.Tasks
{
    internal static class TaskUtility
    {
        /// <summary>
        /// Cached completed task.
        /// </summary>
        /// <returns></returns>
        internal static readonly Task CompletedTask = Task.FromResult(true);

        /// <summary>
        /// Create faulted task with exception.
        /// </summary>
        /// <param name="ex">Exception to put in task.</param>
        /// <returns>Faulted task.</returns>
        internal static Task FromException(Exception ex)
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
            completionSource.TrySetException(ex);
            return completionSource.Task;
        }
    }
}
