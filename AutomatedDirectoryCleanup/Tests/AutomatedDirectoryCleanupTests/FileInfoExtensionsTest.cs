using AutomatedDirectoryCleanup;

namespace AutomatedDirectoryCleanupTests;

[Collection("Directory Collection")]
public class FileInfoExtensionsTest(SharedDirectoryFixture fixture)
{
    [Fact]
    public void ShouldReturnTrueIfFileIsLocked()
    {
        var lockedFilePath = Path.Combine(fixture._testDir, "LockedFile.txt");
        File.WriteAllText(lockedFilePath, "This file should be locked!");
        var lockedFileInfo = new FileInfo(lockedFilePath);
        using var lockStream = lockedFileInfo.Open(FileMode.Open, FileAccess.Read);
        var isLocked = FileInfoExtensions.IsFileLocked(lockedFileInfo);
        Assert.True(isLocked);
        var isLockedGeneric = FileInfoExtensions.IsFileLockedGeneric(lockedFileInfo);
        Assert.True(isLockedGeneric);
    }

    [Fact]
    public void ShouldReturnFalseIfFileIsUnlocked()
    {
        var unlockedFilePath = Path.Combine(fixture._testDir, "UnlockedFile.txt");
        File.WriteAllText(unlockedFilePath, "This file should be unlocked!");
        var unlockedFileInfo = new FileInfo(unlockedFilePath);
        var isLocked = FileInfoExtensions.IsFileLocked(unlockedFileInfo);
        Assert.False(isLocked);
        var isLockedGeneric = FileInfoExtensions.IsFileLockedGeneric(unlockedFileInfo);
        Assert.False(isLockedGeneric);
    }
}
