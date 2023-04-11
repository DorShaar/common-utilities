namespace Temporaries;

public class TempFile : IDisposable
{
    public string Path { get; }

    /// <summary>
    /// Creating a file with random name. 
    /// </summary>
    public TempFile()
    {
        Path = System.IO.Path.GetRandomFileName();
    }

    /// <summary>
    /// Assuming file <see cref="iPath"/> is already created 
    /// </summary>
    public TempFile(string filePath)
    {
        Path = filePath;
    }

    public static TempFile CopyFromExistingFile(string existingFile)
    {
        if (!File.Exists(existingFile))
        {
            throw new FileNotFoundException(existingFile);
        }

        TempFile tempFile = new(System.IO.Path.GetRandomFileName());
        File.Copy(existingFile, tempFile.Path);

        return tempFile;
    }

    public static TempFile CreateZeroBytesTemporaryFile()
    {
        TempFile tempFile = new(System.IO.Path.GetTempFileName());

        return tempFile;
    }

    public void Dispose()
    {
        File.Delete(Path);
    }
}