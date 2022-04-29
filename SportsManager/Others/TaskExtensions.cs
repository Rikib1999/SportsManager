using System.Threading.Tasks;

namespace SportsManager.Others
{
    public static class TaskExtensions
    {
        public async static void Await(this Task task)
        {
            await task;
        }
    }
}
