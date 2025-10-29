using CommandLine;
using CommandLine.Text;

namespace FlattenFolder.Console
{
    public class Options
    {
        [Option('s', "source", Required = true, HelpText = "Source folder path")]
        public required string SourceFolderPath { get; set; }

        [Option('d', "destination", Required = true, HelpText = "Destination folder path")]
        public required string DestinationFolderPath { get; set; }

        [Option('e', "extensions", Required = false, HelpText = "File extensions to search for")]
        public string? FileExtensions { get; set; }

        [Option('i', "image", Required = false, HelpText = "Search for image files")]
        public bool ImageExtensions { get; set; } = false;

        [Option('v', "video", Required = false, HelpText = "Search for video files")]
        public bool VideoExtensions { get; set; } = false;

        [Option('o', "office", Required = false, HelpText = "Search for office document files")]
        public bool OfficeDocumentExtensions { get; set; } = false;

        [Option('a', "audio", Required = false, HelpText = "Search for audio files")]
        public bool AudioExtensions { get; set; } = false;

        [Usage(ApplicationAlias = "FlattenFolder.Console")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example("Search for all files in a folder", new Options { SourceFolderPath = "C:\\Temp", DestinationFolderPath = "C:\\Temp\\Flattened" }),
                    new Example("Search for image files in a folder", new Options { SourceFolderPath = "C:\\Temp", DestinationFolderPath = "C:\\Temp\\Flattened", ImageExtensions = true }),
                    new Example("Search for video files in a folder", new Options { SourceFolderPath = "C:\\Temp", DestinationFolderPath = "C:\\Temp\\Flattened", VideoExtensions = true }),
                    new Example("Search for office document files in a folder", new Options { SourceFolderPath = "C:\\Temp", DestinationFolderPath = "C:\\Temp\\Flattened", OfficeDocumentExtensions = true }),
                    new Example("Search for audio files in a folder", new Options { SourceFolderPath = "C:\\Temp", DestinationFolderPath = "C:\\Temp\\Flattened", AudioExtensions = true }),
                    new Example("Search for specific file extensions in a folder", new Options { SourceFolderPath = "C:\\Temp", DestinationFolderPath = "C:\\Temp\\Flattened", FileExtensions = "txt,pdf" }),
                };
            }
        }
    }
}
