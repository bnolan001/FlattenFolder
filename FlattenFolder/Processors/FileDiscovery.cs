using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace FlattenFolder.Processors
{
    public static class FileDiscovery
    {

        /// <summary>
        /// Searches the <c>folderPath</c> for files that match the <c>regex</c> and returns a dictionary
        /// of file paths and file info.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="regex"></param>
        /// <param name="searchOption"></param>
        public static void RootFolderFileSearch(string folderPath, string regex, SearchOption searchOption)
        {
            var files = Directory.GetFiles(folderPath, "*.*", searchOption);
            var filteredFiles = files.Where(f => Regex.IsMatch(f.ToLower(), regex));
            var fileDataList = new Dictionary<string, List<Models.FileInfo>>();
            foreach (var filePath in filteredFiles)
            {
                Console.WriteLine(filePath);
                var fileName = Path.GetFileName(filePath);

                var fileInfo = new Models.FileInfo
                {
                    Name = fileName,
                    Hash = null,
                    Extension = Path.GetExtension(fileName),
                    Path = filePath
                };

                fileDataList.Add(filePath, new List<Models.FileInfo> { fileInfo });
            }
        }

        /// <summary>
        /// Recursively searches the <c>folderPath</c> for files that match the <c>regex</c> and returns a dictionary of 
        /// file paths and file info.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="regex"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static Dictionary<string, List<Models.FileInfo>> DeduppedRootFolderFileSearch(string folderPath,
            string regex, SearchOption searchOption)
        {
            var files = Directory.GetFiles(folderPath, "*.*", searchOption);
            var filteredFiles = files.Where(f => Regex.IsMatch(f.ToLower(), regex));
            var deduppedFiles = new Dictionary<string, List<Models.FileInfo>>();
            foreach (var filePath in filteredFiles)
            {
                Console.WriteLine(filePath);
                var fileName = Path.GetFileName(filePath);
                var checksum = GetFileChecksum(filePath);
                var fileInfo = new Models.FileInfo
                {
                    Name = fileName,
                    Hash = checksum,
                    Extension = Path.GetExtension(fileName),
                    Path = filePath
                };

                if (deduppedFiles.ContainsKey(checksum))
                {
                    deduppedFiles[checksum].Add(fileInfo);
                }
                else
                {
                    deduppedFiles.Add(checksum, new List<Models.FileInfo> { fileInfo });
                }
            }
            return deduppedFiles;
        }

        /// <summary>
        /// Iterates through the list of <c>files</c> and removes all files that have the same checksum as an ealier file.
        /// </summary>
        /// <param name="files"></param>
        /// <returns>List of unique files</returns>
        public static List<Models.FileInfo> RemoveDuplicates(List<Models.FileInfo> files)
        {

            var deduppedFiles = new Dictionary<string, List<Models.FileInfo>>();
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file.Path);

                var checksum = GetFileChecksum(file.Path);
                var fileInfo = new Models.FileInfo
                {
                    Name = fileName,
                    Hash = checksum,
                    Extension = Path.GetExtension(fileName),
                    Path = file.Path
                };

                if (deduppedFiles.ContainsKey(checksum))
                {
                    deduppedFiles[checksum].Add(fileInfo);
                }
                else
                {
                    deduppedFiles.Add(checksum, new List<Models.FileInfo> { fileInfo });
                }
            }
            var flattenedList = deduppedFiles.Select(x => x.Value.First()).ToList();
            return flattenedList;
        }

        /// <summary>
        /// Gets the SHA256 checksum of a file at <c>filePath</c>
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileChecksum(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate,
                            FileAccess.Read);
            var checksum = CalculateChecksumFromStream(fileStream);
            return checksum;
        }

        /// <summary>
        /// Calculates the SHA256 checksum based off of the file stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string CalculateChecksumFromStream(Stream stream)
        {
            using (var bufferedStream = new BufferedStream(stream, 16 * 1024 * 1024))
            {
                var sha = SHA256.Create();
                byte[] checksum = sha.ComputeHash(bufferedStream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }
    }
}
