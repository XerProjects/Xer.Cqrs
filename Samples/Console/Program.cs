using System.Threading.Tasks;

namespace Console
{
    class Program
    {
        static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            await CommandHandlingDemo();
            await QueryHandlingDemo();
            await EventHandlingDemo();

            System.Console.WriteLine("Press any key to close demo application.");
            System.Console.ReadKey();
        }

        private static async Task CommandHandlingDemo()
        {
            System.Console.WriteLine("-------------------Command Handling Demo Start-------------------");

            await new CommandHandlingDemo.Demo().ExecuteDemoAsync();

            System.Console.WriteLine("-------------------Command Handling Demo End-------------------");
        }

        private static async Task QueryHandlingDemo()
        {
            System.Console.WriteLine("-------------------Query Handling Demo Start-------------------");

            await new QueryHandlingDemo.Demo().ExecuteDemoAsync();

            System.Console.WriteLine("-------------------Query Handling Demo End-------------------");
        }

        private static async Task EventHandlingDemo()
        {
            System.Console.WriteLine("-------------------Event Handling Demo Start-------------------");

            await new EventHandlingDemo.Demo().ExecuteDemoAsync();
            
            System.Console.WriteLine("-------------------Event Handling Demo Start-------------------");
        }
    }
}
