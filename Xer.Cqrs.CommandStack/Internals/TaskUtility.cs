namespace System.Threading.Tasks
{
    internal static class TaskUtility
    {
        internal static readonly Task CompletedTask = Task.FromResult(true);

        internal static void Await(this Task task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        internal static Task CreateFaultedTask(Exception ex)
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
            completionSource.TrySetException(ex);
            return completionSource.Task;
        }
    }
}
