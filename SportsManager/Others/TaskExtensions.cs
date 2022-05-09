using System.Threading.Tasks;

namespace SportsManager.Others
{
    /// <summary>
    /// Class for task extension methods.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Task extension method for awaiting the task.
        /// </summary>
        /// <param name="task">Task to await.</param>
        public async static void Await(this Task task)
        {
            await task;
        }
    }
}
