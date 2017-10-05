namespace System.Threading.Tasks
{
    internal static class TaskUtility
    {
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
