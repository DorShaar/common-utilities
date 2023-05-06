using IOWrapper;
using Xunit;

namespace Tests.IOWrapper;

public class FileSystemPathTests
{
    [Fact]
    public void Ctor_RootedPath_WindowsCharSeparators_PathStringAsExpected()
    {
        FileSystemPath path = new("C:\\dor\\someDir");
        Assert.Equal("C:/dor/someDir", path.PathString);
    }
    
    [Fact]
    public void Ctor_RootedPath_UnixCharSeparators_PathStringAsExpected()
    {
        FileSystemPath path = new("C:/dor/someDir");
        Assert.Equal("C:/dor/someDir", path.PathString);
    }
    
    [Fact]
    public void Ctor_NonRootedPath_HasCharSeparatorAtTheBeginning_WindowsCharSeparators_PathStringAsExpected()
    {
        FileSystemPath path = new("\\dor\\someDir");
        Assert.Equal("/dor/someDir", path.PathString);
    }
    
    [Fact]
    public void Ctor_NonRootedPath_HasCharSeparatorAtTheBeginning_UnixCharSeparators_PathStringAsExpected()
    {
        FileSystemPath path = new("/dor/someDir");
        Assert.Equal("/dor/someDir", path.PathString);
    }
    
    [Fact]
    public void Ctor_NonRootedPath_HasNoCharSeparatorAtTheBeginning_WindowsCharSeparators_PathStringAsExpected()
    {
        FileSystemPath path = new("dor\\someDir");
        Assert.Equal("/dor/someDir", path.PathString);
    }
    
    [Fact]
    public void Ctor_NonRootedPath_HasNoCharSeparatorAtTheBeginning_UnixCharSeparators_PathStringAsExpected()
    {
        FileSystemPath path = new("dor/someDir");
        Assert.Equal("/dor/someDir", path.PathString);
    }
    
    [Fact]
    public void Ctor_HasCharSeparatorAtTheEnd_WindowsCharSeparators_PathStringAsExpected()
    {
        FileSystemPath path = new("dor\\someDir\\");
        Assert.Equal("/dor/someDir", path.PathString);
    }
    
    [Fact]
    public void Ctor_HasCharSeparatorAtTheEnd_UnixCharSeparators_PathStringAsExpected()
    {
        FileSystemPath path = new("dor/someDir/");
        Assert.Equal("/dor/someDir", path.PathString);
    }

    [Fact]
    public void Replace_OnePathHasCharSeparatorAtTheBeginningAndTheOtherDont_ReplacedAsExpected()
    {
        FileSystemPath path = new("C:\\dor\\folder1\\games\\princeOfPersia");
        const string oldValue = "games/princeOfPersia";
        const string newValue = "/nonDosGames/commandos";

        FileSystemPath replacedPath = path.Replace(oldValue, newValue);
        Assert.Equal("C:/dor/folder1/nonDosGames/commandos", replacedPath.PathString);
    }

    [Fact]
    public void GetRelativePath_PathAreNotRelative_ThrowsArgumentException()
    {
        FileSystemPath path = new("C:\\dor\\folder1\\games\\princeOfPersia");
        const string nonRelativeToPath = "nonRelativePath";

        Assert.Throws<ArgumentException>(() => path.GetRelativePath(nonRelativeToPath));
    }
    
    [Fact]
    public void GetRelativePath_RelativeToIsEmpty_ThrowsArgumentException()
    {
        FileSystemPath path = new("C:\\dor\\folder1\\games\\princeOfPersia");
        const string nonRelativeToPath = "";

        Assert.Throws<ArgumentException>(() => path.GetRelativePath(nonRelativeToPath));
    }
    
    [Theory]
    [InlineData("C:\\dor\\folder1\\games\\princeOfPersia", "C:/dor/folder1/", "/games/princeOfPersia")]
    [InlineData("C:/FilesHashesHandlerTests/bin/Debug/net7.0/Games/file in games directory.txt",
        "C:\\FilesHashesHandlerTests\\bin\\Debug\\net7.0",
        "/Games/file in games directory.txt")]
    public void GetRelativePath_PathAreRelative_ReturnsRelativePath(string path, string relativeTo, string expectedPath)
    {
        FileSystemPath fileSystemPath = new(path);

        FileSystemPath relativePath = fileSystemPath.GetRelativePath(relativeTo);
        
        Assert.Equal(expectedPath, relativePath.PathString);
    }
    
    [Fact]
    public void Combine_Path1HasCharSeparatorAtTheBeginning_CombinedAsExpected()
    {
        FileSystemPath path = new("\\dor\\folder1\\games\\princeOfPersia");
        const string path2 = "folder/file";

        FileSystemPath combinedPath = path.Combine(path2);
        Assert.Equal("/dor/folder1/games/princeOfPersia/folder/file", combinedPath.PathString);
    }
    
    [Fact]
    public void Combine_Path2HasCharSeparatorAtTheBeginning_CombinedAsExpected()
    {
        FileSystemPath path = new("C:\\dor\\folder1\\games\\princeOfPersia");
        const string path2 = "/folder/file";

        FileSystemPath combinedPath = path.Combine(path2);
        Assert.Equal("C:/dor/folder1/games/princeOfPersia/folder/file", combinedPath.PathString);
    }

    [Fact]
    public void IsPathRelative_RootedPath_False()
    {
        FileSystemPath rootedPath = new("C:\\dor\\folder1\\games\\princeOfPersia");
        Assert.False(rootedPath.IsPathRelative());
    }
    
    [Fact]
    public void IsPathRelative_NonRootedPath_True()
    {
        FileSystemPath rootedPath = new("\\dor\\folder1\\games\\princeOfPersia");
        Assert.True(rootedPath.IsPathRelative());
    }
}