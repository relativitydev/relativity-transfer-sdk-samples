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

namespace Relativity.Transfer.SDK.Samples.Repository.FullPathWorkflow;

[Sample(SampleOrder.UploadDirectoryWithMaxHttpConnections, "Limiting HTTP connections",
    "The sample illustrates how to limit the number of concurrent HTTP connections used by a transfer, e.g. in network environments with a limited number of TCP connections or ports.",
    typeof(UploadDirectoryWithMaxHttpConnections),
    TransferType.UploadDirectory)]
internal class UploadDirectoryWithMaxHttpConnections : ISample
{
	private readonly IConsoleLogger _consoleLogger;
	private readonly IPathExtension _pathExtension;
	private readonly IRelativityAuthenticationProviderFactory _relativityAuthenticationProviderFactory;
	private readonly IProgressHandlerFactory _progressHandlerFactory;

	public UploadDirectoryWithMaxHttpConnections(IConsoleLogger consoleLogger,
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

		var transferClient = TransferClientBuilder.FullPathWorkflow
			.WithAuthentication(authenticationProvider)
			.WithClientName(clientName)
			.WithStagingExplorerContext()
			.Build();

		_consoleLogger.PrintCreatingTransfer(jobId, source, destination);

		// MaxHttpConnections limits the number of concurrent HTTP connections used by a transfer. Lower it when the network
		// environment offers a limited number of TCP connections or ports (e.g. restrictive firewalls or NAT devices).
		// The default is Environment.ProcessorCount * 8. A value lower than 1 (including the default of the int type, which is 0)
		// or higher than that default throws ArgumentOutOfRangeException - if you do not need to limit the connections, just do not set it.
		// The option applies to FullPathWorkflow transfers to the RelativityOne fileshare; it is ignored by cloud transfers.
		var maxHttpConnectionsOptions = new UploadDirectoryOptions
		{
			MaxHttpConnections = 4
		};

		var result = await transferClient
			.UploadDirectoryAsync(jobId, source, destination, maxHttpConnectionsOptions, progressHandler, token)
			.ConfigureAwait(false);

		_consoleLogger.PrintTransferResult(result);
	}
}
