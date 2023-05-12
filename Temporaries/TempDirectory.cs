namespace Temporaries;

public class TempDirectory : IDisposable
{
    #region Properties
    public string Path { get; }
    #endregion Properties

    public TempDirectory()
    {
        Path = Directory.CreateDirectory(System.IO.Path.GetRandomFileName()).FullName;
    }

    public TempDirectory(string directoryPath)
    {
        Path = Directory.CreateDirectory(directoryPath).FullName;
    }

    public static TempDirectory CreateTemporaryDirectory(string directoryName)
    {
        TempDirectory tempFile = new(directoryName);

        return tempFile;
    }

    public static TempDirectory CreateTemporaryDirectory()
    {
        TempDirectory tempFile = new();

        return tempFile;
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
    
    public override string ToString()
    {
        return Path;
    }
}