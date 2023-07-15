namespace IOWrapper;

public static class DirectoryOperations
{
	public static bool CopyDirectory(string sourceDirectory,
									 string destinationDirectory,
									 string searchPattern,
									 bool shouldOverwriteExist,
									 SearchOption searchOption,
									 CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (!Directory.Exists(sourceDirectory))
		{
			return false;
		}

		if (string.IsNullOrWhiteSpace(searchPattern))
		{
			return false;
		}
		
		if (searchOption == SearchOption.AllDirectories)
		{
			string[] subDirectories = Directory.GetDirectories(sourceDirectory);
			if (subDirectories.Length > 0)
			{
				foreach (string subDirectory in subDirectories)
				{
					string destinationSubDirectory = Path.Combine(destinationDirectory, Path.GetFileName(subDirectory));
					bool isCopySucceed = CopyDirectory(subDirectory,
													   destinationSubDirectory,
													   searchPattern,
													   shouldOverwriteExist,
													   searchOption,
													   cancellationToken);

					if (!isCopySucceed)
					{
						return false;
					}
				}
			}

			searchOption = SearchOption.TopDirectoryOnly;
		}

		_ = Directory.CreateDirectory(destinationDirectory);

		string[] innerFiles = Directory.GetFiles(sourceDirectory, searchPattern, searchOption);
		return copyDirectoryInternal(innerFiles,
									 destinationDirectory,
									 shouldOverwriteExist);
	}
	
	private static bool copyDirectoryInternal(IEnumerable<string> filesToCopy,
											  string destinationDirectory,
											  bool shouldOverwriteExist)

	{
		foreach (string fileToCopy in filesToCopy)
		{
			try
			{
				File.Copy(fileToCopy, Path.Combine(destinationDirectory, Path.GetFileName(fileToCopy)), shouldOverwriteExist);
			}
			catch
			{
				return false;
			}
		}

		return true;
	}
}