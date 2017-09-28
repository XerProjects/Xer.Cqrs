namespace System.Threading.Tasks
{
    internal static class TaskUtility
    {
        public static readonly Task CompletedTask = Task.FromResult(0);

        public static void Await(this Task task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static TResult Await<TResult>(this Task<TResult> task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
