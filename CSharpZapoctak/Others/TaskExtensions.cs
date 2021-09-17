using System.Threading.Tasks;

namespace CSharpZapoctak.Others
{
    public static class TaskExtensions
    {
        public async static void Await(this Task task)
        {
            await task;
        }
    }
}
