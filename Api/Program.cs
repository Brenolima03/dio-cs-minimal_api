using minimal_api;

static IHostBuilder CreateHostBuilder(string[] args)
{
  return Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webHostBuilder =>
    {
      webHostBuilder.UseStartup<Startup>();
    });
}

CreateHostBuilder(args).Build().Run();
