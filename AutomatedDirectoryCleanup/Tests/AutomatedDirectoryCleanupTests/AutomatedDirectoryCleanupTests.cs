using AutomatedDirectoryCleanup;
using Microsoft.Extensions.Logging;
using Moq;

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
        var mockLogger = new Mock<ILogger<DirectoryCleaner>>();

        var testCleanupDir = new CleanupDirectory(_testDir)
        {
            Extensions = ["*"],
            TicksSinceCreation = TimeSpan.FromDays(30).Ticks
        };

        var now = DateTime.Now;

        var testFile1Path = Path.Combine(_testDir, "TestFile1.txt");
        File.WriteAllText(testFile1Path, "This file should be deleted.");
        File.SetCreationTime(testFile1Path, now.AddDays(-31));

        var testFile2Path = Path.Combine(_testDir, "TestFile2.txt");
        File.WriteAllText(testFile2Path, "This file should be deleted.");
        File.SetCreationTime(testFile2Path, now.AddDays(-31));

        var testFile3Path = Path.Combine(_testDir, "TestFile3.txt");
        File.WriteAllText(testFile3Path, "This file should be deleted.");
        File.SetCreationTime(testFile3Path, now.AddDays(-31));

        var testDirectoryCleaner = new DirectoryCleaner(mockLogger.Object);

        var deletedCount = testDirectoryCleaner.DeleteOldFilesByExtension(testCleanupDir);

        Assert.Equal(3, deletedCount);
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
