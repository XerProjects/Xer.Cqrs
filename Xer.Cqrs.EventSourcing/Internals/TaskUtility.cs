namespace System.Threading.Tasks
{
    internal static class TaskUtility
    {
        public static readonly Task CompletedTask = Task.FromResult(true);

        internal static Task FromException(Exception ex)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            taskCompletionSource.TrySetException(ex);
            return taskCompletionSource.Task;
        }

        internal static Task<TResult> FromException<TResult>(Exception ex)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
            taskCompletionSource.TrySetException(ex);
            return taskCompletionSource.Task;
        }
    }
}
