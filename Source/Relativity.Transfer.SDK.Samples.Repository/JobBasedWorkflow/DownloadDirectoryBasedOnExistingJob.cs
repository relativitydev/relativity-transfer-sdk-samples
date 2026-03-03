using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Transfer.SDK.Interfaces.Authentication;
using Relativity.Transfer.SDK.Interfaces.Options;
using Relativity.Transfer.SDK.Interfaces.Paths;
using Relativity.Transfer.SDK.Samples.Core.Attributes;
using Relativity.Transfer.SDK.Samples.Core.Authentication;
using Relativity.Transfer.SDK.Samples.Core.Configuration;
using Relativity.Transfer.SDK.Samples.Core.Helpers;
using Relativity.Transfer.SDK.Samples.Core.ProgressHandler;
using Relativity.Transfer.SDK.Samples.Core.Runner;

namespace Relativity.Transfer.SDK.Samples.Repository.JobBasedWorkflow;

[Sample(SampleOrder.DownloadDirectoryBasedOnExistingJob, "Download a directory (using the job based workflow and based on an existing job)",
	"The sample illustrates how to implement a directory download (using the job based workflow and a source path based on an existing job) from a RelativityOne file share.",
	typeof(DownloadDirectoryBasedOnExistingJob),
	TransferType.DownloadDirectoryBasedOnExistingJob)]
internal class DownloadDirectoryBasedOnExistingJob : ISample
{
	private readonly IConsoleLogger _consoleLogger;
	private readonly IPathExtension _pathExtension;
	private readonly IRelativityAuthenticationProviderFactory _relativityAuthenticationProviderFactory;
	private readonly IProgressHandlerFactory _progressHandlerFactory;

	public DownloadDirectoryBasedOnExistingJob(IConsoleLogger consoleLogger,
		IPathExtension pathExtension,
		IRelativityAuthenticationProviderFactory relativityAuthenticationProviderFactory,
		IProgressHandlerFactory progressHandlerFactory)
	{
		_consoleLogger = consoleLogger;
		_pathExtension = pathExtension;
		_relativityAuthenticationProviderFactory = relativityAuthenticationProviderFactory;
		_progressHandlerFactory = progressHandlerFactory;
	}

	public async Task ExecuteAsync(Configuration configuration, CancellationToken token)
	{
		var clientName = configuration.Common.ClientName;
		var uploadJobId = configuration.Common.JobId;
		var uploadSource = new DirectoryPath(configuration.DownloadDirectoryBasedOnExistingJob.Source);
		var uploadDestination =
			string.IsNullOrWhiteSpace(configuration.DownloadDirectoryBasedOnExistingJob.FirstDestination)
				? _pathExtension.GetDefaultRemoteDirectoryPathForUpload(configuration.Common)
				: new DirectoryPath(configuration.DownloadDirectoryBasedOnExistingJob.FirstDestination);
		var authenticationProvider = _relativityAuthenticationProviderFactory.Create(configuration.Common);
        // This is transfer options object which is not necessary if you do not need change default parameters.
        var uploadDirectoryOptions = new UploadDirectoryOptions()
        {
            MaximumSpeed = default,
            OverwritePolicy = default,
            // ...
        };
        var progressHandler = _progressHandlerFactory.Create();

		// The builder follows the Fluent convention, and more options will be added in the future. The only required component (besides the client name)
		// is the authentication provider - a provided one that utilizes an OAuth-based approach has been provided, but the custom implementation can be created.
		// This sample creates a full path workflow and uploads a directory, then a download transfer job is registered based on the existing upload job (a source path based on this upload).
		var transferFullPathClient = TransferClientBuilder.FullPathWorkflow
			.WithAuthentication(authenticationProvider)
			.WithClientName(clientName)
			.WithStagingExplorerContext()
			.Build();

		_consoleLogger.PrintCreatingTransfer(uploadJobId, uploadSource, uploadDestination);

		var uploadResult = await transferFullPathClient
			.UploadDirectoryAsync(uploadJobId, uploadSource, uploadDestination, uploadDirectoryOptions, progressHandler, token)
            // If you do not need pass transfer options you can invoke this method like this:
            //.UploadDirectoryAsync(uploadJobId, uploadSource, uploadDestination, progressHandler, token)
            .ConfigureAwait(false);

		_consoleLogger.PrintTransferResult(uploadResult, "Upload transfer has finished:", false);

		// Based on the recently finished upload job, create a new download job (a source path based on this upload).
		var downloadJobId = Guid.NewGuid();
		var downloadDestination =
			_pathExtension.EnsureLocalDirectory(configuration.DownloadDirectoryBasedOnExistingJob.SecondDestination);

		await RegisterDownloadJobFromExistingJobAsync(authenticationProvider, downloadJobId, uploadJobId)
			.ConfigureAwait(false);

		// build Transfer SDK client supporting job based workflow
		var transferJobClient = TransferClientBuilder.JobBasedWorkflow
			.WithAuthentication(authenticationProvider)
			.WithClientName(clientName)
			.WithStagingExplorerContext()
			.Build();

		_consoleLogger.PrintCreatingTransfer(downloadJobId, uploadSource, downloadDestination);
        // This is transfer options object which is not necessary if you do not need change default parameters.
        var downloadDirectoryOptions = new DownloadDirectoryOptions()
        {
            MaximumSpeed = default,
            OverwritePolicy = default,
            // ...
        };

        var downloadResult = await transferJobClient
			.DownloadDirectoryAsync(downloadJobId, downloadDestination, downloadDirectoryOptions, progressHandler, token)
            // If you do not need pass transfer options you can invoke this method like this:
            //.DownloadDirectoryAsync(downloadJobId, downloadDestination, progressHandler, token)
            .ConfigureAwait(false);

		_consoleLogger.PrintTransferResult(downloadResult, "Download transfer has finished:");
	}

	/// <summary>
	///     An existing job id can be provided to set up a source path based on the existing job.
	/// </summary>
	private async Task RegisterDownloadJobFromExistingJobAsync(IRelativityAuthenticationProvider authenticationProvider,
		Guid jobId, Guid existingJobId)
	{
		_consoleLogger.PrintRegisteringTransferJob(jobId, existingJobId: existingJobId);

		var jobBuilder = new TransferJobBuilder(authenticationProvider);
		await jobBuilder.FromExistingJob(existingJobId).CreateDownloadJobAsync(jobId)
			.ConfigureAwait(false);
	}
}