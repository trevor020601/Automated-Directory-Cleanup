using AutomatedDirectoryCleanup;

namespace AutomatedDirectoryCleanupTests;

public class FileInfoExtensionsTest : IDisposable
{
    private readonly string _testDir = Path.Combine(Path.GetTempPath(), "FileInfoExtensionsTestDir");
    private bool _disposed;

    public FileInfoExtensionsTest()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }

        Directory.CreateDirectory(_testDir);
    }

    [Fact]
    public void ShouldReturnTrueIfFileIsLocked()
    {
        var lockedFilePath = Path.Combine(_testDir, "LockedFile.txt");
        File.WriteAllText(lockedFilePath, "This file should be locked!");
        var lockedFileInfo = new FileInfo(lockedFilePath);
        using var lockStream = lockedFileInfo.Open(FileMode.Open, FileAccess.Read);
        var isLocked = FileInfoExtensions.IsFileLockedGeneric(lockedFileInfo);
        Assert.True(isLocked);
    }

    [Fact]
    public void ShouldReturnFalseIfFileIsUnlocked()
    {
        var unlockedFilePath = Path.Combine(_testDir, "UnlockedFile.txt");
        File.WriteAllText(unlockedFilePath, "This file should be unlocked!");
        var unlockedFileInfo = new FileInfo(unlockedFilePath);
        var isLocked = FileInfoExtensions.IsFileLocked(unlockedFileInfo);
        Assert.False(isLocked);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (Directory.Exists(_testDir))
                {
                    Directory.Delete(_testDir, true);
                }
            }

            _disposed = true;
        }
    }
}
