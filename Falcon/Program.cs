using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Falcon
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();

            Console.ReadKey();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
    }
}