namespace System.Threading.Tasks
{
    internal static class TaskUtility
    {
        internal static readonly Task CompletedTask = Task.FromResult(true);

        internal static Task FromException(Exception ex)
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
            completionSource.TrySetException(ex);
            return completionSource.Task;
        }
    }
}
