namespace IOWrapper;

public class FileSystemPath
{
    public string PathString { get; }
    
    public FileSystemPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException($"{nameof(path)} is empty");
        }
        
        PathString = PreparePath(path);
    }

    public override string ToString()
    {
        return PathString;
    }

    public FileSystemPath Replace(string oldValue, string newValue)
    {
        FileSystemPath oldPath = new(oldValue); 
        FileSystemPath newPath = new(newValue); 
        string replacedPathString = PathString.Replace(oldPath.PathString, newPath.PathString);
        return new FileSystemPath(replacedPathString);
    }

    public FileSystemPath GetRelativePath(string relativeTo)
    {
        FileSystemPath relativeToPath = new(relativeTo);
        if (!PathString.StartsWith(relativeToPath.PathString))
        {
            throw new ArgumentException($"Path {PathString} is not relative to {relativeTo}");
        }

        string relativePathString = PathString.Replace(relativeToPath.PathString, string.Empty);
        return new FileSystemPath(relativePathString);
    }

    public FileSystemPath Combine(string pathToCombineFromRight)
    {
        return Combine(new FileSystemPath(pathToCombineFromRight));
    }
    
    public FileSystemPath Combine(FileSystemPath pathToCombineFromRight)
    {
        string combinedPath = PathString.TrimEnd('/') + '/' + pathToCombineFromRight.PathString.TrimStart('/');
        return new FileSystemPath(combinedPath);
    }
    
    public bool IsPathRelative()
    {
        return PathString.StartsWith('/') && PathString.Length > 1;
    }
    
    private static string PreparePath(string path)
    {
        string pathWithUnixCharSeparators = path.Replace('\\', '/');
        
        if (!Path.IsPathRooted(pathWithUnixCharSeparators) && pathWithUnixCharSeparators[0] != '/')
        {
            pathWithUnixCharSeparators = '/' + pathWithUnixCharSeparators;
        }

        return pathWithUnixCharSeparators.TrimEnd('/');
    }
}