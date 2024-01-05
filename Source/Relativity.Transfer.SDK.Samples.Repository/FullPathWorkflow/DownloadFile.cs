﻿using System.Threading;
using System.Threading.Tasks;
using Relativity.Transfer.SDK.Interfaces.Paths;
using Relativity.Transfer.SDK.Samples.Core.Attributes;
using Relativity.Transfer.SDK.Samples.Core.Authentication;
using Relativity.Transfer.SDK.Samples.Core.Configuration;
using Relativity.Transfer.SDK.Samples.Core.Helpers;
using Relativity.Transfer.SDK.Samples.Core.ProgressHandler;
using Relativity.Transfer.SDK.Samples.Core.Runner;

namespace Relativity.Transfer.SDK.Samples.Repository.FullPathWorkflow;

[Sample(8, "Download a file",
    "The sample illustrates how to implement a file download from a RelativityOne file share.",
    typeof(DownloadFile),
    TransferType.DownloadFile)]
internal class DownloadFile(
    IConsoleLogger consoleLogger,
    IPathExtension pathExtension,
    IRelativityAuthenticationProviderFactory relativityAuthenticationProviderFactory,
    IProgressHandlerFactory progressHandlerFactory)
    : ISample
{
    public async Task ExecuteAsync(Configuration configuration, CancellationToken token)
    {
        var clientName = configuration.Common.ClientName;
        var jobId = configuration.Common.JobId;
        var source = new FilePath(configuration.DownloadFile.Source);
        var destination = pathExtension.EnsureLocalDirectory(configuration.DownloadFile.Destination);
        var authenticationProvider = relativityAuthenticationProviderFactory.Create(configuration.Common);
        var progressHandler = progressHandlerFactory.Create();

        // The builder follows the Fluent convention, and more options will be added in the future. The only required component (besides the client name)
        // is the authentication provider - a provided one that utilizes an OAuth-based approach has been provided, but the custom implementation can be created.
        var transferClient = TransferClientBuilder.FullPathWorkflow
            .WithAuthentication(authenticationProvider)
            .WithClientName(clientName)
            .Build();

        consoleLogger.PrintCreatingTransfer(jobId, source, destination);

        var result = await transferClient
            .DownloadFileAsync(jobId, source, destination, progressHandler, token)
            .ConfigureAwait(false);

        consoleLogger.PrintTransferResult(result);
    }
}