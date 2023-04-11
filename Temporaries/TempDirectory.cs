using System;
using System.IO;

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

    public static TempDirectory CreateTemporaryDirectory(string iDirName)
    {
        TempDirectory tempFile = new(iDirName);

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
}