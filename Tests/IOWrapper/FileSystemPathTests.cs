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
    
    [Fact]
    public void GetRelativePath_PathAreRelative_ReturnsRelativePath()
    {
        FileSystemPath path = new("C:\\dor\\folder1\\games\\princeOfPersia");
        const string relativeTo = "C:/dor/folder1/";

        FileSystemPath relativePath = path.GetRelativePath(relativeTo);
        
        Assert.Equal("/games/princeOfPersia", relativePath.PathString);
    }
}