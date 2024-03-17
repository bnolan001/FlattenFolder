using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace FlattenFolder.Processors
{
    public static class FileDiscovery
    {

        public static void RootFolderFileSearch(string folderPath, string regex, SearchOption searchOption)
        {
            var files = Directory.GetFiles(folderPath, "*.*", searchOption);
            var filteredFiles = files.Where(f => Regex.IsMatch(f.ToLower(), regex));
            var fileDataList = new Dictionary<string, List<FlattenFolder.Models.FileInfo>>();
            foreach (var filePath in filteredFiles)
            {
                Console.WriteLine(filePath);
                var fileName = Path.GetFileName(filePath);

                var fileInfo = new FlattenFolder.Models.FileInfo
                {
                    Name = fileName,
                    Hash = null,
                    Extension = Path.GetExtension(fileName),
                    Path = filePath
                };

                fileDataList.Add(filePath, new List<FlattenFolder.Models.FileInfo> { fileInfo });
            }
        }

        public static Dictionary<string, List<Models.FileInfo>> DeduppedRootFolderFileSearch(string folderPath, string regex, SearchOption searchOption)
        {
            var files = Directory.GetFiles(folderPath, "*.*", searchOption);
            var filteredFiles = files.Where(f => Regex.IsMatch(f.ToLower(), regex));
            var deduppedFiles = new Dictionary<string, List<Models.FileInfo>>();
            foreach (var filePath in filteredFiles)
            {
                Console.WriteLine(filePath);
                var fileName = Path.GetFileName(filePath);
                var checksum = GetFileChecksum(filePath);
                var fileInfo = new FlattenFolder.Models.FileInfo
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

        public static List<Models.FileInfo> RemoveDuplicates(List<Models.FileInfo> files)
        {

            var deduppedFiles = new Dictionary<string, List<Models.FileInfo>>();
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file.Path);

                var checksum = GetFileChecksum(file.Path);
                var fileInfo = new FlattenFolder.Models.FileInfo
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
                    deduppedFiles.Add(checksum, new List<FlattenFolder.Models.FileInfo> { fileInfo });
                }
            }
            var flattenedList = deduppedFiles.Select(x => x.Value.FirstOrDefault()).ToList();
            return flattenedList;
        }

        public static string GetFileChecksum(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate,
                            FileAccess.Read);
            var checksum = GetChecksumBuffered(fileStream);
            return checksum;
        }


        public static string GetChecksumBuffered(Stream stream)
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
