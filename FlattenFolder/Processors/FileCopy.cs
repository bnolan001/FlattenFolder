namespace FlattenFolder.Processors
{
    public static class FileCopy
    {
        public static List<Models.FileInfo> CopyFilesToFolder(List<Models.FileInfo> files, string folderPath, bool createFolderIfNotExists)
        {
            var filesNotCopied = new List<Models.FileInfo>();
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (Models.FileInfo file in files)
            {
                var newFilePath = Path.Combine(folderPath, file.Name);
                if (!File.Exists(newFilePath))
                {
                    File.Copy(file.Path, newFilePath);
                }
                else
                {
                    var index = 0;
                    do
                    {
                        var fileName = string.IsNullOrEmpty(file.Extension) ? file.Name : file.Name.Substring(0, file.Name.Length - file.Extension.Length);
                        fileName = $"{fileName}_{index}{file.Extension}";
                        newFilePath = Path.Combine(folderPath, fileName);
                        if (!File.Exists(newFilePath))
                        {
                            File.Copy(file.Path, newFilePath);
                            break;
                        }
                        index++;
                    } while (index < Int32.MaxValue);
                    if (index == Int32.MaxValue)
                    {
                        filesNotCopied.Add(file);
                    }

                }
            }
            return filesNotCopied;
        }
    }
}
