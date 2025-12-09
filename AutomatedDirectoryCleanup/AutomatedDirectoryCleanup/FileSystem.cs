namespace AutomatedDirectoryCleanup;

public class FileSystem : IFileSystem
{
    public FileInfo[] GetFiles(string dirPath)
    {
        return new DirectoryInfo(dirPath).GetFiles();
    }
}
