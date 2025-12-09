namespace AutomatedDirectoryCleanup;

public interface IFileSystem
{
    FileInfo[] GetFiles(string dirPath);
}
