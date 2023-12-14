namespace FileWatcherService;

using System.IO.Hashing;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
///
/// </summary>
/// <remarks>
///     REFERENCES:
///     -   <see href="https://learn.microsoft.com/en-us/dotnet/core/extensions/windows-service?pivots=dotnet-8-0" />
/// </remarks>
public class FileWatcherBackgroundService : BackgroundService
{
    private readonly ILogger<FileWatcherBackgroundService> _logger;

    private readonly FileHashingProvider _hashingProvider = new();

    private string _fileHash = string.Empty;

    public FileWatcherBackgroundService(ILogger<FileWatcherBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var folderPath = @"C:\TEMP\TEMP\AdGuard\";
            var fileName = @"AdGuard URL Tracking Filter - Test - #111 Good.txt";
            var filePath = Path.Combine(folderPath, fileName);
            _fileHash = _hashingProvider.ComputeHash(filePath);

            var fileSystemWatcher = new FileSystemWatcher(folderPath);

            fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName |
                                             NotifyFilters.FileName |
                                             NotifyFilters.Security |
                                             NotifyFilters.Size |
                                             NotifyFilters.Attributes |
                                             NotifyFilters.CreationTime |
                                             NotifyFilters.LastWrite |
                                             NotifyFilters.LastAccess;

            fileSystemWatcher.Created += FileSystemWatcherOnChanged;
            fileSystemWatcher.Changed += FileSystemWatcherOnChanged;
            fileSystemWatcher.Deleted += FileSystemWatcherOnChanged;

            fileSystemWatcher.Filter = fileName;
            fileSystemWatcher.IncludeSubdirectories = false;
            fileSystemWatcher.EnableRaisingEvents = true;

            while (!stoppingToken.IsCancellationRequested)
            {
                //await Task.Yield();
                await Task.Delay(10, stoppingToken).ConfigureAwait(false);

                //string joke = _jokeService.GetJoke();
                //_logger.LogWarning("{Joke}", joke);

                //await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            fileSystemWatcher?.Dispose();
        }
        catch (OperationCanceledException)
        {
            // When the stopping token is canceled, for example, a call made from services.msc,
            // we shouldn't exit with a non-zero exit code. In other words, this is expected...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);

            // Terminates this process and returns an exit code to the operating system.
            // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
            // performs one of two scenarios:
            // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
            // 2. When set to "StopHost": will cleanly stop the host, and log errors.
            //
            // In order for the Windows Service Management system to leverage configured
            // recovery options, we need to terminate the process with a non-zero exit code.
            Environment.Exit(1);
        }
    }

    private void FileSystemWatcherOnChanged(object sender, FileSystemEventArgs e)
    {
        var message = $"File change notification triggered at: {e.FullPath} ({e.ChangeType})";
        _logger.LogInformation(message);

        var fileHash = _hashingProvider.ComputeHash(e.FullPath);
        if (fileHash != _fileHash)
        {
            message = $"File content change detected at: {e.FullPath} (XxHash3: {_fileHash} -> {fileHash})";
            _logger.LogInformation(message);
        }
        else
        {
            message = $"No file content change detected at: {e.FullPath} ({nameof(XxHash3)}: {fileHash})";
            _logger.LogInformation(message);

            message = $"File change notification dismissed for: {e.FullPath}";
            _logger.LogInformation(message);
        }
    }
}
