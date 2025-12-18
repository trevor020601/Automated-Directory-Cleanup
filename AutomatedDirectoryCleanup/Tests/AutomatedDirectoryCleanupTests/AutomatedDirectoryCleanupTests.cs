using AutomatedDirectoryCleanup;

namespace AutomatedDirectoryCleanupTests;

public class AutomatedDirectoryCleanupTests : IDisposable
{
    private readonly string _testDir = Path.Combine(Path.GetTempPath(), "AutomatedDirectoryCleanupTestDir");

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
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }
}
