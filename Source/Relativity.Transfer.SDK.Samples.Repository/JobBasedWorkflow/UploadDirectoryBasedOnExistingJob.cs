using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Transfer.SDK.Interfaces.Options;
using Relativity.Transfer.SDK.Interfaces.Paths;
using Relativity.Transfer.SDK.Samples.Core.Attributes;
using Relativity.Transfer.SDK.Samples.Core.Authentication;
using Relativity.Transfer.SDK.Samples.Core.Configuration;
using Relativity.Transfer.SDK.Samples.Core.Helpers;
using Relativity.Transfer.SDK.Samples.Core.ProgressHandler;
using Relativity.Transfer.SDK.Samples.Core.Runner;

namespace Relativity.Transfer.SDK.Samples.Repository.JobBasedWorkflow;

[Sample(SampleOrder.UploadDirectoryBasedOnExistingJob, "Upload a directory (using the job based workflow and based on an existing job)",
	"The sample illustrates how to implement a directory upload (using the job based workflow and a destination path based on an existing job) to a RelativityOne file share.",
	typeof(UploadDirectoryBasedOnExistingJob),
	TransferType.UploadDirectoryBasedOnExistingJob)]
internal class UploadDirectoryBasedOnExistingJob : ISample
{
	private readonly IConsoleLogger _consoleLogger;
	private readonly IPathExtension _pathExtension;
	private readonly IRelativityAuthenticationProviderFactory _relativityAuthenticationProviderFactory;
	private readonly IProgressHandlerFactory _progressHandlerFactory;

	public UploadDirectoryBasedOnExistingJob(IConsoleLogger consoleLogger,
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
		var firstJobId = configuration.Common.JobId;
		// A new job, based on an existing job, should be registered with a unique job ID.
		var secondJobId = Guid.NewGuid();
		var firstSource = new DirectoryPath(configuration.UploadDirectoryBasedOnExistingJob.FirstSource);
		var destination = string.IsNullOrWhiteSpace(configuration.UploadDirectoryBasedOnExistingJob.Destination)
			? _pathExtension.GetDefaultRemoteDirectoryPathForUpload(configuration.Common)
			: new DirectoryPath(configuration.UploadDirectoryBasedOnExistingJob.Destination);
		// To enhance the visual representation of the newly transferred data alongside the previously transferred data by the preceding job, it is advisable to utilize a unique source data set.
		var secondSource = new DirectoryPath(configuration.UploadDirectoryBasedOnExistingJob.SecondSource);
		var authenticationProvider = _relativityAuthenticationProviderFactory.Create(configuration.Common);
		var jobBuilder = new TransferJobBuilder(authenticationProvider);
        // This is transfer options object which is not necessary if you do not need change default parameters.
        var firstUploadDirectoryOptions = new UploadDirectoryOptions()
        {
            MaximumSpeed = default,
            OverwritePolicy = default,
            // ...
        };
        var progressHandler = _progressHandlerFactory.Create();

		await RegisterUploadDirectoryJobAsync(jobBuilder, firstJobId, destination).ConfigureAwait(false);

		// The builder follows the Fluent convention, and more options will be added in the future. The only required component (besides the client name)
		// is the authentication provider - a provided one that utilizes an OAuth-based approach has been provided, but the custom implementation can be created.
		// The Transfer SDK client builder creates a job based workflow.
		var transferClient = TransferClientBuilder.JobBasedWorkflow
			.WithAuthentication(authenticationProvider)
			.WithClientName(clientName)
			.WithStagingExplorerContext()
			.Build();

		_consoleLogger.PrintCreatingTransfer(firstJobId, firstSource, destination);

		var firstResult = await transferClient
			.UploadDirectoryAsync(firstJobId, firstSource, firstUploadDirectoryOptions, progressHandler, token)
            // If you do not need pass transfer options you can invoke this method like this:
            //.UploadDirectoryAsync(firstJobId, firstSource, progressHandler, token)
            .ConfigureAwait(false);

		_consoleLogger.PrintTransferResult(firstResult, "First transfer has finished:", false);

		await RegisterUploadJobFromExistingJobAsync(jobBuilder, secondJobId, firstJobId).ConfigureAwait(false);

        // This is transfer options object which is not necessary if you do not need change default parameters.
        var secondUploadDirectoryOptions = new UploadDirectoryOptions()
        {
            MaximumSpeed = default,
            OverwritePolicy = default,
            // ...
        };
        
		var secondResult = await transferClient
			.UploadDirectoryAsync(secondJobId, secondSource, secondUploadDirectoryOptions, progressHandler, token)
            // If you do not need pass transfer options you can invoke this method like this:
            //.UploadDirectoryAsync(secondJobId, secondSource, progressHandler, token)
            .ConfigureAwait(false);

		_consoleLogger.PrintTransferResult(secondResult, "Second transfer has finished:");
	}

	/// <summary>
	///     An existing job id can be provided to set up a destination path based on the existing job.
	/// </summary>
	private async Task RegisterUploadJobFromExistingJobAsync(TransferJobBuilder jobBuilder, Guid jobId,
		Guid existingJobId)
	{
		_consoleLogger.PrintRegisteringTransferJob(jobId, existingJobId: existingJobId);

		await jobBuilder.FromExistingJob(existingJobId).CreateUploadJobAsync(jobId)
			.ConfigureAwait(false);
	}

	/// <summary>
	///     In the job based workflow a new job with the specified destination path needs to be registered before the transfer
	///     can be started.
	/// </summary>
	private async Task RegisterUploadDirectoryJobAsync(TransferJobBuilder jobBuilder, Guid firstUploadJobId,
		DirectoryPath destination)
	{
		_consoleLogger.PrintRegisteringTransferJob(firstUploadJobId, destination: destination);

		await jobBuilder.NewJob().CreateUploadDirectoryJobAsync(firstUploadJobId, destination).ConfigureAwait(false);
	}
}