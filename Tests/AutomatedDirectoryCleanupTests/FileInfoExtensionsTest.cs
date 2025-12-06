using AutomatedDirectoryCleanup;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;

namespace AutomatedDirectoryCleanupTests;

public class FileInfoExtensionsTest
{
    [Fact]
    public void ShouldReturnTrueIfFileIsLocked()
    {
        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile(@"C:\test.txt", new MockFileData("This file is locked!"));
        var mockFileInfo = mockFileSystem.FileInfo.New(@"C:\test.txt");
        var testFileInfo = new FileInfo(mockFileInfo.Name);
        //var file = mockFileSystem.GetFile(@"C:\test.txt");
        //var testFileSystem = new TestFileSystem(new System.IO.Abstractions.FileSystem());
        using var lockStream = testFileInfo.Open(FileMode.Open, FileAccess.Read);
        var isLocked = FileInfoExtensions.IsFileLocked(testFileInfo);
        Assert.True(isLocked);
    }

    [Fact]
    public void ShouldReturnFalseIfFileIsUnlocked()
    {

        //var isLocked = FileInfoExtensions.IsFileLocked();
        //Assert.False(isLocked);
    }

    public class TestFileSystem(System.IO.Abstractions.IFileSystem fileSystem)
    {
        public IFileInfo GetFileInfo(string path)
        {
            return fileSystem.FileInfo.New(path);
        }
    }
}
