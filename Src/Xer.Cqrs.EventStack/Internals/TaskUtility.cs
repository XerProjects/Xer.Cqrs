using System.Collections.Generic;
using System.Linq;

namespace System.Threading.Tasks
{
    internal class TaskUtility
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

        /// <summary> 
        /// Returns a sequence of tasks which will be observed to complete with the same set 
        /// of results as the given input tasks, but in the order in which the original tasks complete. 
        /// </summary> 
        /// <remarks>https://codeblog.jonskeet.uk/2012/01/16/eduasync-part-19-ordering-by-completion-ahead-of-time/</remarks>
        internal static IEnumerable<Task> OrderByCompletion(IEnumerable<Task> inputTasks)
        {
            // Copy the input so we know it’ll be stable, and we don’t evaluate it twice 
            List<Task> inputTaskList = inputTasks.ToList();
           
            var completionSourceList = new List<TaskCompletionSource<bool>>(inputTaskList.Count);
            for (int i = 0; i < inputTaskList.Count; i++)
            {
                completionSourceList.Add(new TaskCompletionSource<bool>());
            }

            // At any one time, this is "the index of the box we’ve just filled". 
            // It would be nice to make it nextIndex and start with 0, but Interlocked.Increment 
            // returns the incremented value… 
            int prevIndex = -1;

            // We don’t have to create this outside the loop, but it makes it clearer 
            // that the continuation is the same for all tasks. 
            Action<Task> continuation = completedTask =>
            {
                int index = Interlocked.Increment(ref prevIndex);
                TaskCompletionSource<bool> source = completionSourceList[index];
                PropagateResult(completedTask, source);
            };

            foreach (Task inputTask in inputTaskList)
            {
                inputTask.ContinueWith(continuation, TaskContinuationOptions.ExecuteSynchronously);
            }

            return completionSourceList.Select(source => source.Task);
        }

        /// <summary> 
        /// Propagates the status of the given task (which must be completed) to a task completion source 
        /// (which should not be). 
        /// </summary> 
        private static void PropagateResult(Task completedTask, TaskCompletionSource<bool> completionSource)
        {
            switch (completedTask.Status)
            {
                case TaskStatus.Canceled:
                    completionSource.TrySetCanceled();
                    break;
                case TaskStatus.Faulted:
                    completionSource.TrySetException(completedTask.Exception.InnerExceptions);
                    break;
                case TaskStatus.RanToCompletion:
                    completionSource.TrySetResult(true);
                    break;
                default:
                    // TODO: Work out whether this is really appropriate. Could set 
                    // an exception in the completion source, of course… 
                    throw new ArgumentException("Task was not completed");
            }
        }
    }
}
