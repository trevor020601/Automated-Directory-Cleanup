using AutomatedDirectoryCleanup;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Testing;

namespace Benchmarks;

public class DirectoryCleanerBenchmarks
{
    private readonly string _testDir = Path.Combine(Path.GetTempPath(), "AutomatedDirectoryCleanupTestDir");

    // TODO: This setup needs to be better in case I want to experiment more with writing benchmarks for this...
    [GlobalSetup]
    public void SetupCleanupDirectory()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }

        Directory.CreateDirectory(_testDir);

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
    }

    [Benchmark]
    public int ParallelDirectoryCleaner()
    {
        var fakeLogger = new FakeLogger<DirectoryCleaner>();

        var testCleanupDir = new CleanupDirectory(_testDir)
        {
            Extensions = ["*"],
            TicksSinceCreation = TimeSpan.FromDays(30).Ticks
        };

        var testDirectoryCleaner = new DirectoryCleaner(fakeLogger);

        var deletedCount = testDirectoryCleaner.DeleteOldFilesByExtension(testCleanupDir);

        return deletedCount;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }
}
