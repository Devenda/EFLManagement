using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace EFLManagementAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>()
                   .ConfigureLogging((hostingContext, logging) =>
                   {
                       logging.AddConsole();
                       logging.AddDebug();
                       logging.AddLog4Net();//https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore default config in log4net.config
                   });
    }
}
