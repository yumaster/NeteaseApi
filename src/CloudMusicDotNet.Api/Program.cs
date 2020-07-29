using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace CloudMusicDotNet.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath).AddJsonFile("api.json", true, true);
            })
            .UseKestrel(opts =>
            {
                opts.Limits.MaxRequestBodySize = 524288000;
            })
            .UseIISIntegration()
            .UseStartup<Startup>();
    }
}
