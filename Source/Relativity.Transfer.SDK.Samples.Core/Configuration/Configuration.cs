using System;

namespace Relativity.Transfer.SDK.Samples.Core.Configuration;

internal record Configuration(
	CommonConfiguration Common,
	SourceAndDestinationConfiguration UploadFile,
	SourceAndDestinationConfiguration UploadDirectory,
	SourceAndDestinationConfiguration CloudUpload,
	SourceAndDestinationConfiguration DownloadFile,
	SourceAndDestinationConfiguration DownloadDirectory,
	SourceAndWorkspaceIdConfiguration UploadDirectoryByWorkspaceId,
	TwoSourcesAndDestinationConfiguration UploadDirectoryBasedOnExistingJob,
	SourceAndTwoDestinationsConfiguration DownloadDirectoryBasedOnExistingJob)
{
	public void Deconstruct(out CommonConfiguration common, out SourceAndDestinationConfiguration uploadFile,
		out SourceAndDestinationConfiguration uploadDirectory, out SourceAndDestinationConfiguration cloudUpload, 
		out SourceAndDestinationConfiguration downloadFile,
		out SourceAndDestinationConfiguration downloadDirectory,
		out SourceAndWorkspaceIdConfiguration uploadDirectoryByWorkspaceId,
		out TwoSourcesAndDestinationConfiguration uploadDirectoryBasedOnExistingJob,
		out SourceAndTwoDestinationsConfiguration downloadDirectoryBasedOnExistingJob)
	{
		common = Common;
		uploadFile = UploadFile;
		uploadDirectory = UploadDirectory;
		cloudUpload = CloudUpload;
		downloadFile = DownloadFile;
		downloadDirectory = DownloadDirectory;
		uploadDirectoryByWorkspaceId = UploadDirectoryByWorkspaceId;
		uploadDirectoryBasedOnExistingJob = UploadDirectoryBasedOnExistingJob;
		downloadDirectoryBasedOnExistingJob = DownloadDirectoryBasedOnExistingJob;
	}

	internal static Configuration ForUploadDirectory(CommonConfiguration common,
		SourceAndDestinationConfiguration uploadDirectory)
	{
		return new Configuration(common,
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			uploadDirectory,
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndWorkspaceIdConfiguration(string.Empty, -1),
			new TwoSourcesAndDestinationConfiguration(string.Empty, string.Empty, string.Empty),
			new SourceAndTwoDestinationsConfiguration(string.Empty, string.Empty, string.Empty));
	}
	
	internal static Configuration ForCloudUpload(CommonConfiguration common,
		SourceAndDestinationConfiguration cloudUpload)
	{
		return new Configuration(common,
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			cloudUpload,
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndWorkspaceIdConfiguration(string.Empty, -1),
			new TwoSourcesAndDestinationConfiguration(string.Empty, string.Empty, string.Empty),
			new SourceAndTwoDestinationsConfiguration(string.Empty, string.Empty, string.Empty));
	}

	internal static Configuration ForUploadFile(CommonConfiguration common,
		SourceAndDestinationConfiguration uploadFile)
	{
		return new Configuration(common,
			uploadFile,
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),

			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndWorkspaceIdConfiguration(string.Empty, -1),
			new TwoSourcesAndDestinationConfiguration(string.Empty, string.Empty, string.Empty),
			new SourceAndTwoDestinationsConfiguration(string.Empty, string.Empty, string.Empty));
	}

	internal static Configuration ForUploadItems(CommonConfiguration common,
		SourceAndDestinationConfiguration uploadsInfoFile)
	{
		return new Configuration(common,
			uploadsInfoFile,
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndWorkspaceIdConfiguration(string.Empty, -1),
			new TwoSourcesAndDestinationConfiguration(string.Empty, string.Empty, string.Empty),
			new SourceAndTwoDestinationsConfiguration(string.Empty, string.Empty, string.Empty));
	}

	internal static Configuration ForDownloadDirectory(CommonConfiguration common,
		SourceAndDestinationConfiguration downloadDirectory)
	{
		return new Configuration(common,
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			downloadDirectory,
			new SourceAndWorkspaceIdConfiguration(string.Empty, -1),
			new TwoSourcesAndDestinationConfiguration(string.Empty, string.Empty, string.Empty),
			new SourceAndTwoDestinationsConfiguration(string.Empty, string.Empty, string.Empty));
	}

	internal static Configuration ForDownloadFile(CommonConfiguration common,
		SourceAndDestinationConfiguration downloadFile)
	{
		return new Configuration(common,
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			downloadFile,
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndWorkspaceIdConfiguration(string.Empty, -1),
			new TwoSourcesAndDestinationConfiguration(string.Empty, string.Empty, string.Empty),
			new SourceAndTwoDestinationsConfiguration(string.Empty, string.Empty, string.Empty));
	}

	internal static Configuration ForUploadDirectoryByWorkspaceId(CommonConfiguration common,
		SourceAndWorkspaceIdConfiguration uploadDirectory)
	{
		return new Configuration(common,
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			uploadDirectory,
			new TwoSourcesAndDestinationConfiguration(string.Empty, string.Empty, string.Empty),
			new SourceAndTwoDestinationsConfiguration(string.Empty, string.Empty, string.Empty));
	}

	internal static Configuration ForUploadDirectoryBasedOnExistingJob(CommonConfiguration common,
		TwoSourcesAndDestinationConfiguration uploadDirectory)
	{
		return new Configuration(common,
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndWorkspaceIdConfiguration(string.Empty, -1),
			uploadDirectory,
			new SourceAndTwoDestinationsConfiguration(string.Empty, string.Empty, string.Empty));
	}

	internal static Configuration ForDownloadDirectoryBasedOnExistingJob(CommonConfiguration common,
		SourceAndTwoDestinationsConfiguration downloadDirectory)
	{
		return new Configuration(common,
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndDestinationConfiguration(string.Empty, string.Empty),
			new SourceAndWorkspaceIdConfiguration(string.Empty, -1),
			new TwoSourcesAndDestinationConfiguration(string.Empty, string.Empty, string.Empty),
			downloadDirectory);
	}
}