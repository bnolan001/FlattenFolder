// See https://aka.ms/new-console-template for more information
using FlattenFolder.Processors;

Console.WriteLine("What folder to flatten");
var folderPath = Console.ReadLine();

Console.WriteLine("What folder should the pictures be copied to");
var copyToFolderPath = Console.ReadLine();
var fileDataList = FileDiscovery.DeduppedRootFolderFileSearch(folderPath, "/*.(jpg|jpeg|png)$", System.IO.SearchOption.AllDirectories);

var duplicates = fileDataList.Where(f => f.Value.Count > 1);
Console.WriteLine("-----------------");
Console.WriteLine($"Total duplicate files found is {duplicates.Count()}.");
foreach (var duplicate in fileDataList)
{
    if (duplicate.Value.Count > 1)
    {
        Console.WriteLine($"Duplicate found: {String.Join(", ", duplicate.Value.Select(f => f.Name).ToList())}");
        foreach (var file in duplicate.Value)
        {
            Console.WriteLine(file);
        }
    }
}
var filesToCopy = fileDataList.Select(x => x.Value.FirstOrDefault()).ToList();
var filesNotCopied = FileCopy.CopyFilesToFolder(filesToCopy, copyToFolderPath, true);
Console.WriteLine("-----------------");
Console.WriteLine($"Total files not copied {filesNotCopied.Count()}.");

foreach (var file in filesNotCopied)
{
    Console.WriteLine(file.Path);
}
