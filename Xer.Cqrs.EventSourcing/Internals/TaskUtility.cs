namespace System.Threading.Tasks
{
    internal static class TaskUtility
    {
        public static readonly Task CompletedTask = Task.FromResult(0);

        public static void RunInBackground(Action action, Action<Exception> exceptionHandler = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch(Exception ex)
                {
                    exceptionHandler?.Invoke(ex);
                }
            });
        }

        public static void RunInBackground(Func<Task> asyncAction, Action<Exception> exceptionHandler = null)
        {
            if (asyncAction == null)
            {
                throw new ArgumentNullException(nameof(asyncAction));
            }

            Task.Run(async () =>
            {
                try
                {
                    await asyncAction().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptionHandler?.Invoke(ex);
                }
            });
        }

        public static void HandleAnyExceptions(this Task task, Action<Exception> exceptionHandler)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.ContinueWith(t => exceptionHandler?.Invoke(t.Exception), 
                                   TaskContinuationOptions.OnlyOnFaulted);
        }

        public static void Await(this Task task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static TResult AwaitResult<TResult>(this Task<TResult> task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

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
