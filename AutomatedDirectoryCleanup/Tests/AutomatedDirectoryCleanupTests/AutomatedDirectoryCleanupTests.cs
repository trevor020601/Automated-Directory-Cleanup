using AutomatedDirectoryCleanup;
using Microsoft.Extensions.Logging.Testing;

namespace AutomatedDirectoryCleanupTests;

[Collection("Directory Collection")]
public class AutomatedDirectoryCleanupTests(SharedDirectoryFixture fixture)
{
    [Fact]
    public void ShouldThrowExceptionIfPathIsTooLong()
    {
        var erroneousPath = @"C:\this\will\throw\an\exception".PadRight(300);
        Assert.Throws<PathTooLongException>(() => new CleanupDirectory(erroneousPath));
    }

    [Fact]
    public void ShouldDeleteAllFilesInDirectory()
    {
        var fakeLogger = new FakeLogger<DirectoryCleaner>();

        var testCleanupDir = new CleanupDirectory(fixture._testDir)
        {
            Extensions = ["*"],
            TicksSinceCreation = TimeSpan.FromDays(30).Ticks
        };

        var now = DateTime.Now;

        var testFile1Path = Path.Combine(fixture._testDir, "TestFile1.txt");
        File.WriteAllText(testFile1Path, "This file should be deleted.");
        File.SetCreationTime(testFile1Path, now.AddDays(-31));

        var testFile2Path = Path.Combine(fixture._testDir, "TestFile2.txt");
        File.WriteAllText(testFile2Path, "This file should be deleted.");
        File.SetCreationTime(testFile2Path, now.AddDays(-31));

        var testFile3Path = Path.Combine(fixture._testDir, "TestFile3.txt");
        File.WriteAllText(testFile3Path, "This file should be deleted.");
        File.SetCreationTime(testFile3Path, now.AddDays(-31));

        var testDirectoryCleaner = new DirectoryCleaner(fakeLogger);

        testDirectoryCleaner.DeleteOldFilesByExtension(testCleanupDir);
        
        var fileCount = new DirectoryInfo(fixture._testDir).EnumerateFiles().Count();

        Assert.Equal(0, fileCount);
    }

    [Fact]
    public void ShouldDeleteUnlockedFilesAndSkipLockedFilesInDirectory()
    {
        var fakeLogger = new FakeLogger<DirectoryCleaner>();

        var testCleanupDir = new CleanupDirectory(fixture._testDir)
        {
            Extensions = ["*"],
            TicksSinceCreation = TimeSpan.FromDays(30).Ticks
        };

        var now = DateTime.Now;

        var testFile1Path = Path.Combine(fixture._testDir, "TestFile1.txt");
        File.WriteAllText(testFile1Path, "This file should be deleted.");
        File.SetCreationTime(testFile1Path, now.AddDays(-31));

        var testFile2Path = Path.Combine(fixture._testDir, "TestFile2.txt");
        File.WriteAllText(testFile2Path, "This file should be locked.");
        File.SetCreationTime(testFile2Path, now.AddDays(-31));
        using var lockStream = File.OpenRead(testFile2Path);

        var testFile3Path = Path.Combine(fixture._testDir, "TestFile3.txt");
        File.WriteAllText(testFile3Path, "This file should be deleted.");
        File.SetCreationTime(testFile3Path, now.AddDays(-31));

        var testDirectoryCleaner = new DirectoryCleaner(fakeLogger);

        Assert.Throws<AggregateException>(() => testDirectoryCleaner.DeleteOldFilesByExtension(testCleanupDir));

        var fileCount = new DirectoryInfo(fixture._testDir).EnumerateFiles().Count();

        Assert.Equal(1, fileCount);
    }

    [Fact]
    public void ShouldOnlyDeleteFilesWithTxtExtension()
    {
        var fakeLogger = new FakeLogger<DirectoryCleaner>();

        var testCleanupDir = new CleanupDirectory(fixture._testDir)
        {
            Extensions = ["txt"],
            TicksSinceCreation = TimeSpan.FromDays(30).Ticks
        };

        var now = DateTime.Now;

        var testFile1Path = Path.Combine(fixture._testDir, "TestFile1.tmp");
        File.WriteAllText(testFile1Path, "This file should be not deleted.");
        File.SetCreationTime(testFile1Path, now.AddDays(-31));

        var testFile2Path = Path.Combine(fixture._testDir, "TestFile2.txt");
        File.WriteAllText(testFile2Path, "This file should be deleted.");
        File.SetCreationTime(testFile2Path, now.AddDays(-31));

        var testFile3Path = Path.Combine(fixture._testDir, "TestFile3.log");
        File.WriteAllText(testFile3Path, "This file should be not deleted.");
        File.SetCreationTime(testFile3Path, now.AddDays(-31));

        var testDirectoryCleaner = new DirectoryCleaner(fakeLogger);

        testDirectoryCleaner.DeleteOldFilesByExtension(testCleanupDir);

        var fileCount = new DirectoryInfo(fixture._testDir).EnumerateFiles().Count();

        Assert.Equal(2, fileCount);
    }

    [Fact]
    public void ShouldOnlyDeleteFilesWithTxtAndTmpExtensions()
    {
        var fakeLogger = new FakeLogger<DirectoryCleaner>();

        var testCleanupDir = new CleanupDirectory(fixture._testDir)
        {
            Extensions = ["txt", "tmp"],
            TicksSinceCreation = TimeSpan.FromDays(30).Ticks
        };

        var now = DateTime.Now;

        var testFile1Path = Path.Combine(fixture._testDir, "TestFile1.tmp");
        File.WriteAllText(testFile1Path, "This file should be not deleted.");
        File.SetCreationTime(testFile1Path, now.AddDays(-31));

        var testFile2Path = Path.Combine(fixture._testDir, "TestFile2.txt");
        File.WriteAllText(testFile2Path, "This file should be deleted.");
        File.SetCreationTime(testFile2Path, now.AddDays(-31));

        var testFile3Path = Path.Combine(fixture._testDir, "TestFile3.log");
        File.WriteAllText(testFile3Path, "This file should be not deleted.");
        File.SetCreationTime(testFile3Path, now.AddDays(-31));

        var testDirectoryCleaner = new DirectoryCleaner(fakeLogger);

        testDirectoryCleaner.DeleteOldFilesByExtension(testCleanupDir);

        var fileCount = new DirectoryInfo(fixture._testDir).EnumerateFiles().Count();

        Assert.Equal(1, fileCount);
    }

    [Fact]
    public void ShouldOnlyDeleteFilesOlderThanTicksSinceCreation()
    {
        var fakeLogger = new FakeLogger<DirectoryCleaner>();

        var testCleanupDir = new CleanupDirectory(fixture._testDir)
        {
            Extensions = ["*"],
            TicksSinceCreation = TimeSpan.FromDays(30).Ticks
        };

        var now = DateTime.Now;

        var testFile1Path = Path.Combine(fixture._testDir, "TestFile1.txt");
        File.WriteAllText(testFile1Path, "This file should be deleted.");
        File.SetCreationTime(testFile1Path, now.AddDays(-31));

        var testFile2Path = Path.Combine(fixture._testDir, "TestFile2.txt");
        File.WriteAllText(testFile2Path, "This file should not be deleted.");

        var testFile3Path = Path.Combine(fixture._testDir, "TestFile3.txt");
        File.WriteAllText(testFile3Path, "This file should be deleted.");
        File.SetCreationTime(testFile3Path, now.AddDays(-31));

        var testDirectoryCleaner = new DirectoryCleaner(fakeLogger);

        testDirectoryCleaner.DeleteOldFilesByExtension(testCleanupDir);

        var fileCount = new DirectoryInfo(fixture._testDir).EnumerateFiles().Count();

        Assert.Equal(1, fileCount);
    }
}
