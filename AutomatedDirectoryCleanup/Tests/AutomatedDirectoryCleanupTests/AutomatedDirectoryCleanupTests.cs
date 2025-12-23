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

    [Fact(Skip = "This only works on Windows OS sadly; Figure out how to fix for Linux.")]
    public void ShouldDeleteUnlockedFilesAndSkipLockedFilesInDirectory()
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
        File.WriteAllText(testFile2Path, "This file should be locked.");
        File.SetCreationTime(testFile2Path, now.AddDays(-31));
        using var lockStream = File.OpenRead(testFile2Path);

        var testFile3Path = Path.Combine(_testDir, "TestFile3.txt");
        File.WriteAllText(testFile3Path, "This file should be deleted.");
        File.SetCreationTime(testFile3Path, now.AddDays(-31));

        var testDirectoryCleaner = new DirectoryCleaner(mockLogger.Object);

        var deletedCount = testDirectoryCleaner.DeleteOldFilesByExtension(testCleanupDir);

        Assert.Equal(2, deletedCount);
    }

    [Fact]
    public void ShouldOnlyDeleteFilesWithTxtExtension()
    {
        var mockLogger = new Mock<ILogger<DirectoryCleaner>>();

        var testCleanupDir = new CleanupDirectory(_testDir)
        {
            Extensions = ["txt"],
            TicksSinceCreation = TimeSpan.FromDays(30).Ticks
        };

        var now = DateTime.Now;

        var testFile1Path = Path.Combine(_testDir, "TestFile1.tmp");
        File.WriteAllText(testFile1Path, "This file should be not deleted.");
        File.SetCreationTime(testFile1Path, now.AddDays(-31));

        var testFile2Path = Path.Combine(_testDir, "TestFile2.txt");
        File.WriteAllText(testFile2Path, "This file should be deleted.");
        File.SetCreationTime(testFile2Path, now.AddDays(-31));

        var testFile3Path = Path.Combine(_testDir, "TestFile3.log");
        File.WriteAllText(testFile3Path, "This file should be not deleted.");
        File.SetCreationTime(testFile3Path, now.AddDays(-31));

        var testDirectoryCleaner = new DirectoryCleaner(mockLogger.Object);

        var deletedCount = testDirectoryCleaner.DeleteOldFilesByExtension(testCleanupDir);

        Assert.Equal(1, deletedCount);
    }

    [Fact]
    public void ShouldOnlyDeleteFilesWithTxtAndTmpExtensions()
    {
        var mockLogger = new Mock<ILogger<DirectoryCleaner>>();

        var testCleanupDir = new CleanupDirectory(_testDir)
        {
            Extensions = ["txt", "tmp"],
            TicksSinceCreation = TimeSpan.FromDays(30).Ticks
        };

        var now = DateTime.Now;

        var testFile1Path = Path.Combine(_testDir, "TestFile1.tmp");
        File.WriteAllText(testFile1Path, "This file should be not deleted.");
        File.SetCreationTime(testFile1Path, now.AddDays(-31));

        var testFile2Path = Path.Combine(_testDir, "TestFile2.txt");
        File.WriteAllText(testFile2Path, "This file should be deleted.");
        File.SetCreationTime(testFile2Path, now.AddDays(-31));

        var testFile3Path = Path.Combine(_testDir, "TestFile3.log");
        File.WriteAllText(testFile3Path, "This file should be not deleted.");
        File.SetCreationTime(testFile3Path, now.AddDays(-31));

        var testDirectoryCleaner = new DirectoryCleaner(mockLogger.Object);

        var deletedCount = testDirectoryCleaner.DeleteOldFilesByExtension(testCleanupDir);

        Assert.Equal(2, deletedCount);
    }

    [Fact]
    public void ShouldOnlyDeleteFilesOlderThanTicksSinceCreation()
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
        File.WriteAllText(testFile2Path, "This file should not be deleted.");

        var testFile3Path = Path.Combine(_testDir, "TestFile3.txt");
        File.WriteAllText(testFile3Path, "This file should be deleted.");
        File.SetCreationTime(testFile3Path, now.AddDays(-31));

        var testDirectoryCleaner = new DirectoryCleaner(mockLogger.Object);

        var deletedCount = testDirectoryCleaner.DeleteOldFilesByExtension(testCleanupDir);

        Assert.Equal(2, deletedCount);
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
