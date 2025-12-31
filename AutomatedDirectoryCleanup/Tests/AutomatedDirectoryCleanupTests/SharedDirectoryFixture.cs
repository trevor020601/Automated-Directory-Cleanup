namespace AutomatedDirectoryCleanupTests;

public class SharedDirectoryFixture : IDisposable
{
    internal readonly string _testDir = Path.Combine(Path.GetTempPath(), "AutomatedDirectoryCleanupTestDir");
    private bool _disposed;

    public SharedDirectoryFixture()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }

        Directory.CreateDirectory(_testDir);
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
