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

[Sample(SampleOrder.UploadFile, "Upload a file",
    "The sample illustrates how to implement a file upload to a RelativityOne file share.",
    typeof(UploadFile),
    TransferType.UploadFile)]
internal class UploadFile : ISample
{
	private readonly IConsoleLogger _consoleLogger;
	private readonly IPathExtension _pathExtension;
	private readonly IRelativityAuthenticationProviderFactory _relativityAuthenticationProviderFactory;
	private readonly IProgressHandlerFactory _progressHandlerFactory;

	public UploadFile(IConsoleLogger consoleLogger,
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
        var source = new FilePath(configuration.UploadFile.Source);
        var destination = string.IsNullOrWhiteSpace(configuration.UploadFile.Destination)
            ? _pathExtension.GetDefaultRemoteDirectoryPathForUpload(configuration.Common)
            : new DirectoryPath(configuration.UploadFile.Destination);
        var authenticationProvider = _relativityAuthenticationProviderFactory.Create(configuration.Common);
        // This is transfer options object which is not necessary if you do not need change default parameters.
        var uploadFileOptions = new UploadFileOptions()
        {
            MaximumSpeed = default,
            OverwritePolicy = default,
            // ...
        };
        var progressHandler = _progressHandlerFactory.Create();

        // The builder follows the Fluent convention, and more options will be added in the future. The only required component (besides the client name)
        // is the authentication provider - a provided one that utilizes an OAuth-based approach has been provided, but the custom implementation can be created.
        var transferClient = TransferClientBuilder.FullPathWorkflow
            .WithAuthentication(authenticationProvider)
            .WithClientName(clientName)
            .WithStagingExplorerContext()
            .Build();

        _consoleLogger.PrintCreatingTransfer(jobId, source, destination);

        var result = await transferClient
            .UploadFileAsync(jobId, source, destination, uploadFileOptions, progressHandler, token)
            // If you do not need pass transfer options you can invoke this method like this:
            //.UploadFileAsync(jobId, source, destination, progressHandler, token)
            .ConfigureAwait(false);

        _consoleLogger.PrintTransferResult(result);
    }
}