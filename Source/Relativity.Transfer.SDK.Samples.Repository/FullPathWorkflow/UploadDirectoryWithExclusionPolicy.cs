using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

[Sample(SampleOrder.UploadDirectoryWithExclusionPolicy, "An exclusion policy",
    "The sample illustrates the implementation of an exclusion policy.",
    typeof(UploadDirectoryWithExclusionPolicy),
    TransferType.UploadDirectory)]
internal class UploadDirectoryWithExclusionPolicy : ISample
{
	private readonly IConsoleLogger _consoleLogger;
	private readonly IPathExtension _pathExtension;
	private readonly IRelativityAuthenticationProviderFactory _relativityAuthenticationProviderFactory;
	private readonly IProgressHandlerFactory _progressHandlerFactory;

	public UploadDirectoryWithExclusionPolicy(IConsoleLogger consoleLogger,
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
        var source = string.IsNullOrWhiteSpace(configuration.UploadDirectory.Source)
            ? CreateTemporaryDirectoryWithFiles(jobId.ToString(), "file1.xls", "file2.bin", "file3.doc", "file4.exe",
                "file5.txt", "file6.xml")
            : new DirectoryPath(configuration.UploadDirectory.Source);
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

        // An exclusion policy implementation should be assigned to a transfer options.
        // The exclusion policy in that case accepts only files with .xls, .doc, and .txt extensions.
        var fileExclusionPolicyOptions = new UploadDirectoryOptions
        {
            ExclusionPolicy = new AcceptExtensionExclusionPolicy(_consoleLogger, new[] { ".xls", ".doc", ".txt" })
        };

        _consoleLogger.PrintCreatingTransfer(jobId, source, destination);

        var result = await transferClient
            .UploadDirectoryAsync(jobId, source, destination, fileExclusionPolicyOptions, progressHandler, token)
            // If you do not need pass transfer options you can invoke this method like this:
            //.UploadDirectoryAsync(jobId, source, destination, progressHandler, token)
            .ConfigureAwait(false);

        _consoleLogger.PrintTransferResult(result);
    }

    // Creates data that contains files which should be excluded by the exclusion policy.
    private DirectoryPath CreateTemporaryDirectoryWithFiles(string directoryName, params string[] fileNames)
    {
        return _pathExtension.CreateDirectoryWithFiles(directoryName, fileNames);
    }

    // An exclusion policy that accepts only files with specified extensions.
    // The interface Relativity.Transfer.SDK.Interfaces.Options.Policies.IFileExclusionPolicy interface needs to be implemented.
    private sealed class AcceptExtensionExclusionPolicy : IFileExclusionPolicy
    {
        private readonly string[] _acceptedFileExtensions;
        private readonly IConsoleLogger _consoleLogger1;

        public AcceptExtensionExclusionPolicy(IConsoleLogger consoleLogger,
	        IEnumerable<string> acceptedFileExtensions)
        {
	        _consoleLogger1 = consoleLogger;
	        _acceptedFileExtensions = acceptedFileExtensions?.ToArray() ?? Array.Empty<string>();
        }

        public Task<bool> ShouldExcludeAsync(IFileReference fileReference)
        {
            var extension = Path.GetExtension(fileReference.AbsolutePath);
            var shouldExclude = !HasAcceptedExtensionOrAcceptAll(extension);
            LogResult(Path.GetFileName(fileReference.AbsolutePath), shouldExclude);

            return Task.FromResult(shouldExclude);
        }

        private void LogResult(string fileName, bool shouldExclude)
        {
            _consoleLogger1.Info($"    --> File [gray]{fileName}[/] was {GetResultMarkup(shouldExclude)}");
        }

        private static string GetResultMarkup(bool shouldExclude)
        {
            return shouldExclude ? "[red]excluded[/]" : "[green]accepted[/]";
        }

        private bool HasAcceptedExtensionOrAcceptAll(string extension)
        {
            return _acceptedFileExtensions.Length == 0 ||
                   _acceptedFileExtensions.Any(x =>
                       string.Compare(x, extension, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}