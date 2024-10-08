﻿using System;
using Relativity.Transfer.SDK.Interfaces.Paths;
using Relativity.Transfer.SDK.Samples.Core.Configuration;
using Relativity.Transfer.SDK.Samples.Core.DTOs;

namespace Relativity.Transfer.SDK.Samples.Core.Helpers;

internal interface IPathExtension
{
	DirectoryPath GetDefaultRemoteDirectoryPathForUpload(CommonConfiguration common);
	string GetDefaultRemoteDirectoryPathForUploadAsString(CommonConfiguration common);
	FilePath CreateTemporarySourceFile();
	DirectoryPath EnsureLocalDirectory(string path);
	DirectoryPath GetDefaultRemoteDirectoryPathForDownload(CommonConfiguration common);
	string GetDefaultRemoteDirectoryPathForDownloadAsString(CommonConfiguration common);

	DirectoryPath GetDestinationDirectoryPathByFileShareInfo(FileShareInfo fileShareInfo, string fileShareRelativePath,
		Guid jobId);

	string GetFileShareRootPath(string uncPath);

	DirectoryPath CreateTemporaryDirectoryWithFiles(Guid jobId);
	DirectoryPath CreateDirectoryWithFiles(string directoryName, params string[] files);
}