using AutomatedDirectoryCleanup;

namespace AutomatedDirectoryCleanupTests;

public class AutomatedDirectoryCleanupTests : IDisposable
{
    private readonly string _testDir = Path.Combine(Path.GetTempPath(), "AutomatedDirectoryCleanupTestDir");
    private bool _disposed;

    public AutomatedDirectoryCleanupTests()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }

        Directory.CreateDirectory(_testDir);
    }

    [Fact]
    public void ShouldDeleteAllFilesInDirectory()
    {

    }

    [Fact]
    public void ShouldDeleteUnlockedFilesAndSkipLockedFilesInDirectory()
    {

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
