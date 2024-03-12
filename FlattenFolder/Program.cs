// See https://aka.ms/new-console-template for more information
using System.Security.Cryptography;
using System.Text.RegularExpressions;

Console.WriteLine("What folder to flatten");
var folderPath = Console.ReadLine();

var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
var filteredFiles = files.Where(f => Regex.IsMatch(f.ToLower(), "/*.(jpg|jpeg|png)$"));
var fileDataList = new Dictionary<string, List<FlattenFolder.Models.FileInfo>>();
foreach (var file in filteredFiles)
{
    Console.WriteLine(file);
    var fileName = Path.GetFileName(file);
    var fileStream = new FileStream(file, FileMode.OpenOrCreate,
                FileAccess.Read);
    var checksum = GetChecksumBuffered(fileStream);
    var fileInfo = new FlattenFolder.Models.FileInfo
    {
        FileName = fileName,
        Hash = checksum,
        Extension = Path.GetExtension(fileName),
        Path = file
    };

    if (fileDataList.ContainsKey(checksum))
    {
        fileDataList[checksum].Add(fileInfo);
    }
    else
    {
        fileDataList.Add(checksum, new List<FlattenFolder.Models.FileInfo> { fileInfo });
    }
}

var duplicates = fileDataList.Where(f => f.Value.Count > 1);

foreach (var duplicate in fileDataList)
{
    if (duplicate.Value.Count > 1)
    {
        Console.WriteLine($"Duplicate found: {String.Join(", ", duplicate.Value.Select(f => f.FileName).ToList())}");
        foreach (var file in duplicate.Value)
        {
            Console.WriteLine(file);
        }
    }
}
static string GetChecksumBuffered(Stream stream)
{
    using (var bufferedStream = new BufferedStream(stream, 16 * 1024 * 1024))
    {
        var sha = SHA256.Create();
        byte[] checksum = sha.ComputeHash(bufferedStream);
        return BitConverter.ToString(checksum).Replace("-", String.Empty);
    }
}