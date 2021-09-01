using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace PrintIt.WebHost
{
	public class Program
	{
		public static int Main(string[] args)
		{
            var config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
             .Build();

            // Logging Setup
            // https://github.com/serilog/serilog-settings-configuration
            // https://github.com/serilog/serilog-sinks-elasticsearch

            Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(config)
               .CreateLogger();

            try
            {
                Log.Information("Starting web host for AMS.Companies.API");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "PrintIt.API Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				}).UseSerilog();
	}
}
