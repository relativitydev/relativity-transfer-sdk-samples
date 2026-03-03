using System;
using System.Collections.Generic;
using System.IO;
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

[Sample(SampleOrder.UploadItems, 
    "Upload items",
    "The sample illustrates how to implement files upload to a RelativityOne file share. ", 
    typeof(UploadItems), 
    TransferType.UploadItems)]
internal class UploadItems : ISample
{
    private readonly IConsoleLogger _consoleLogger;
    private readonly IPathExtension _pathExtension;
    private readonly IRelativityAuthenticationProviderFactory _relativityAuthenticationProviderFactory;
    private readonly IProgressHandlerFactory _progressHandlerFactory;

    public UploadItems(IConsoleLogger consoleLogger, IPathExtension pathExtension, IRelativityAuthenticationProviderFactory relativityAuthenticationProviderFactory, IProgressHandlerFactory progressHandlerFactory)
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
        var uploadListOfItemsOptions = new UploadListOfItemsOptions()
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
        
        try
        {
            var sources = GetTransferredEntities(configuration.UploadFile.Source);
            var result = await transferClient
                .UploadItemsAsync(jobId, sources, destination, uploadListOfItemsOptions, progressHandler, token)
                // If you do not need pass transfer options you can invoke this method like this:
                //.UploadItemsAsync(jobId, sources, destination, progressHandler, token)
                .ConfigureAwait(false);
            
            _consoleLogger.PrintTransferResult(result);
        }
        catch (Exception e)
        {
            _consoleLogger.PrintError(e);
        }
    }

    private async IAsyncEnumerable<TransferEntity> GetTransferredEntities(string filePath)
    {
        const int maxSourceFiles = 5;
        using var sr = new StreamReader(filePath);

        for (var i = 0; i < maxSourceFiles && await sr.ReadLineAsync() is { } line; i++)
        {
            var paths = line.Split(';');
            if (paths.Length != 2)
            {
                _consoleLogger.Info($"[red]Invalid parameters in {i} line[/]");
                continue;
            }

            var transferEntity = new TransferEntity(new FilePath(paths[0]), new FilePath(paths[1]));
            
            yield return transferEntity;
        }
    }
}