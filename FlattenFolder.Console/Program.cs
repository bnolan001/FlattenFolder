using CommandLine;
using FlattenFolder.Console;
using FlattenFolder.Processors;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning)
        .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
        .AddConsole();
});
ILogger logger = loggerFactory.CreateLogger<Program>();

Console.WriteLine("Flatten Folder Console App");
var results = Parser.Default.ParseArguments<Options>(args).MapResult(o => ExecuteFlattening(o), errors => HandleErrors(errors));

if (results < 0)
{
    Console.Error.WriteLine("Flatten Folder Console App ended with errors, check the logs for more details.");
    Environment.ExitCode = results;
}

int ExecuteFlattening(Options o)
{
    logger.LogDebug($"Source folder path: {o.SourceFolderPath}");
    logger.LogDebug($"Destination folder path: {o.DestinationFolderPath}");
    logger.LogDebug($"File extensions: {o.FileExtensions}");
    logger.LogDebug($"Image extensions: {o.ImageExtensions}");
    logger.LogDebug($"Video extensions: {o.VideoExtensions}");
    logger.LogDebug($"Office document extensions: {o.OfficeDocumentExtensions}");
    logger.LogDebug($"Audio extensions: {o.AudioExtensions}");

    if (string.IsNullOrEmpty(o.FileExtensions)
        && !o.ImageExtensions
        && !o.VideoExtensions
        && !o.OfficeDocumentExtensions
        && !o.AudioExtensions)
    {
        logger.LogError("No file extensions provided to search for.");
        return -1;
    }
    List<string> extensions = GetFileExtensions(o);

    logger.LogDebug("Starting at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    try
    {
        var fileDataList = FileDiscovery.DeduppedRootFolderFileSearch(o.SourceFolderPath, $"/*.({string.Join("|", extensions)})", SearchOption.AllDirectories);

        var duplicates = fileDataList.Where(f => f.Value.Count > 1);
        logger.LogDebug($"Total duplicate files found is {duplicates.Count()}.");

        foreach (var duplicate in duplicates)
        {
            if (duplicate.Value.Count > 1)
            {
                logger.LogDebug($"Duplicate found: {String.Join(", ", duplicate.Value.Select(f => f.Name).ToList())}");
                foreach (var file in duplicate.Value)
                {
                    logger.LogDebug($" - {file.Path}");
                }
            }
        }
        var filesToCopy = fileDataList.Select(x => x.Value.FirstOrDefault()).ToList();
        var filesNotCopied = FileCopy.CopyFilesToFolder(filesToCopy, o.DestinationFolderPath, true);
        logger.LogDebug("Completed at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        logger.LogDebug($"Total files not copied {filesNotCopied.Count()}.");
        logger.LogDebug($"Total files copied {filesToCopy.Count()}.");

        foreach (var file in filesNotCopied)
        {
            logger.LogDebug($" - {file.Path}");
        }

        return 1;
    }
    catch (Exception ex)
    {
        logger.LogError("An error occurred: {0}", ex.Message);
        return -1;
    }
}

int HandleErrors(IEnumerable<Error> errors)
{
    var result = -2;
    logger.LogError("Total number of errors in the command line{0}", errors.Count());
    foreach (var error in errors)
    {
        logger.LogError("Error: {0}", error.Tag);
    }
    if (errors.Any(x => x is HelpRequestedError || x is VersionRequestedError))
        result = -1;
    logger.LogError("Exit code {0}", result);
    return result;
}

List<string> GetFileExtensions(Options o)
{
    List<string> extensions = new List<string>();
    if (!string.IsNullOrEmpty(o.FileExtensions))
    {
        extensions.AddRange(o.FileExtensions.Split([',', '|'], StringSplitOptions.RemoveEmptyEntries));
    }
    if (o.ImageExtensions)
    {
        extensions.AddRange(FlattenFolder.Enums.FileExtensions.ImageExtensions);
    }
    if (o.VideoExtensions)
    {
        extensions.AddRange(FlattenFolder.Enums.FileExtensions.VideoExtensions);
    }
    if (o.OfficeDocumentExtensions)
    {
        extensions.AddRange(FlattenFolder.Enums.FileExtensions.OfficeDocumentExtensions);
    }
    if (o.AudioExtensions)
    {
        extensions.AddRange(FlattenFolder.Enums.FileExtensions.AudioExtensions);
    }
    return extensions;
}