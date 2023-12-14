using FileWatcherService;

using NLog;
using NLog.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddNLog();
//LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "FileWatcherService";
});

builder.Services.AddHostedService<FileWatcherBackgroundService>();

var host = builder.Build();

host.Run();

LogManager.Shutdown();




//var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddHostedService<Worker>();

//var host = builder.Build();
//host.Run();
