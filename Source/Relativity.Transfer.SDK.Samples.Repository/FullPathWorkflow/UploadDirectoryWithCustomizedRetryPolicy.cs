using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Transfer.SDK.Interfaces.Options;
using Relativity.Transfer.SDK.Interfaces.Options.Policies;
using Relativity.Transfer.SDK.Interfaces.Paths;
using Relativity.Transfer.SDK.Samples.Core.Attributes;
using Relativity.Transfer.SDK.Samples.Core.Authentication;
using Relativity.Transfer.SDK.Samples.Core.Configuration;
using Relativity.Transfer.SDK.Samples.Core.Helpers;
using Relativity.Transfer.SDK.Samples.Core.ProgressHandler;
using Relativity.Transfer.SDK.Samples.Core.Runner;

namespace Relativity.Transfer.SDK.Samples.Repository.FullPathWorkflow;

[Sample(SampleOrder.UploadDirectoryWithCustomizedRetryPolicy, "A retry policy",
    "The sample illustrates the implementation of a retry policy to enhance the resilience of a transfer.",
    typeof(UploadDirectoryWithCustomizedRetryPolicy),
    TransferType.UploadDirectory)]
internal class UploadDirectoryWithCustomizedRetryPolicy : ISample
{
	private readonly IConsoleLogger _consoleLogger;
	private readonly IPathExtension _pathExtension;
	private readonly IRelativityAuthenticationProviderFactory _relativityAuthenticationProviderFactory;
	private readonly IProgressHandlerFactory _progressHandlerFactory;

	public UploadDirectoryWithCustomizedRetryPolicy(IConsoleLogger consoleLogger,
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
        var progressHandler = _progressHandlerFactory.Create();

        // The builder follows the Fluent convention, and more options will be added in the future. The only required component (besides the client name)
        // is the authentication provider - a provided one that utilizes an OAuth-based approach has been provided, but the custom implementation can be created.
        var transferClient = TransferClientBuilder.FullPathWorkflow
            .WithAuthentication(authenticationProvider)
            .WithClientName(clientName)
            .WithStagingExplorerContext()
            .Build();

        _consoleLogger.PrintCreatingTransfer(jobId, source, destination);

        // The exponential retry policy as well as linear policy are policies that can be used to enhance the resilience of a transfer.
        // The retry policy implementation should be assigned to a transfer options.
        var exponentialRetryPolicyOptions = new UploadDirectoryOptions
        {
            TransferRetryPolicyDefinition = TransferRetryPolicyDefinition.ExponentialPolicy(TimeSpan.FromSeconds(2), 2)
        };

        var result = await transferClient
            .UploadDirectoryAsync(jobId, source, destination, exponentialRetryPolicyOptions, progressHandler, token)
            // If you do not need pass transfer options you can invoke this method like this:
            //.UploadDirectoryAsync(jobId, source, destination, progressHandler, token)
            .ConfigureAwait(false);

        _consoleLogger.PrintTransferResult(result);
    }
}