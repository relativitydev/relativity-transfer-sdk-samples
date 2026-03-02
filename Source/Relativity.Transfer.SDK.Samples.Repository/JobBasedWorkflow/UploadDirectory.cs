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

[Sample(SampleOrder.UploadDirectoryJobBasedWorkflow, "Upload a directory (using the job based workflow)",
	"The sample illustrates how to implement a directory upload (using the job based workflow) to a RelativityOne file share.",
	typeof(UploadDirectory),
	TransferType.UploadDirectory)]
internal sealed class UploadDirectory : ISample
{
	private readonly IConsoleLogger _consoleLogger;
	private readonly IPathExtension _pathExtension;
	private readonly IRelativityAuthenticationProviderFactory _relativityAuthenticationProviderFactory;
	private readonly IProgressHandlerFactory _progressHandlerFactory;

	public UploadDirectory(IConsoleLogger consoleLogger,
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
		var jobId = configuration.Common.JobId;
		var source = new DirectoryPath(configuration.UploadDirectory.Source);
		var destination = string.IsNullOrWhiteSpace(configuration.UploadDirectory.Destination)
			? _pathExtension.GetDefaultRemoteDirectoryPathForUpload(configuration.Common)
			: new DirectoryPath(configuration.UploadDirectory.Destination);
		var authenticationProvider = _relativityAuthenticationProviderFactory.Create(configuration.Common);
        // This is transfer options object which is not necessary if you do not need change default parameters.
        var uploadDirectoryOptions = new UploadDirectoryOptions()
        {
            MaximumSpeed = default,
            OverwritePolicy = default,
            // ...
        };
        var progressHandler = _progressHandlerFactory.Create();

		await RegisterUploadDirectoryJobAsync(authenticationProvider, jobId, destination).ConfigureAwait(false);

		// The builder follows the Fluent convention, and more options will be added in the future. The only required component (besides the client name)
		// is the authentication provider - a provided one that utilizes an OAuth-based approach has been provided, but the custom implementation can be created.
		// The Transfer SDK client builder creates a job based workflow.
		var transferClient = TransferClientBuilder.JobBasedWorkflow
			.WithAuthentication(authenticationProvider)
			.WithClientName(clientName)
			.WithStagingExplorerContext()
			.Build();

		_consoleLogger.PrintCreatingTransfer(jobId, source, destination);

		var result = await transferClient
			.UploadDirectoryAsync(jobId, source, uploadDirectoryOptions, progressHandler, token)
            // If you do not need pass transfer options you can invoke this method like this:
            //.UploadDirectoryAsync(jobId, source, progressHandler, token)
            .ConfigureAwait(false);

		_consoleLogger.PrintTransferResult(result);
	}

	/// <summary>
	///     In the job based workflow a new job with the specified destination path needs to be registered before the transfer
	///     can be started.
	/// </summary>
	private async Task RegisterUploadDirectoryJobAsync(IRelativityAuthenticationProvider authenticationProvider,
		Guid jobId, DirectoryPath destination)
	{
		_consoleLogger.PrintRegisteringTransferJob(jobId, destination: destination);

		var jobBuilder = new TransferJobBuilder(authenticationProvider);
		await jobBuilder.NewJob().CreateUploadDirectoryJobAsync(jobId, destination).ConfigureAwait(false);
	}
}