using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Relativity.Transfer.SDK.Interfaces.Options;
using Relativity.Transfer.SDK.Interfaces.Paths;
using Relativity.Transfer.SDK.Samples.Core.Attributes;
using Relativity.Transfer.SDK.Samples.Core.Authentication;
using Relativity.Transfer.SDK.Samples.Core.Configuration;
using Relativity.Transfer.SDK.Samples.Core.DTOs;
using Relativity.Transfer.SDK.Samples.Core.Helpers;
using Relativity.Transfer.SDK.Samples.Core.ProgressHandler;
using Relativity.Transfer.SDK.Samples.Core.Runner;
using Relativity.Transfer.SDK.Samples.Core.UI;

namespace Relativity.Transfer.SDK.Samples.Repository.FullPathWorkflow;

[Sample(SampleOrder.UploadToFileSharePathBasedOnWorkspaceId, "A transfer based on a WorkspaceId",
    "The sample illustrates the implementation of a transfer based on a WorkspaceId.",
    typeof(UploadToFileSharePathBasedOnWorkspaceId),
    TransferType.UploadDirectoryByWorkspaceId)]
internal class UploadToFileSharePathBasedOnWorkspaceId : ISample
{
	private readonly IConsoleLogger _consoleLogger;
	private readonly IPathExtension _pathExtension;
	private readonly IRelativityAuthenticationProviderFactory _relativityAuthenticationProviderFactory;
	private readonly IProgressHandlerFactory _progressHandlerFactory;
	private readonly IBearerTokenRetriever _bearerTokenRetriever;
	private readonly IFileShareSelectorMenu _fileShareSelectorMenu;

	public UploadToFileSharePathBasedOnWorkspaceId(IConsoleLogger consoleLogger,
		IPathExtension pathExtension,
		IRelativityAuthenticationProviderFactory relativityAuthenticationProviderFactory,
		IProgressHandlerFactory progressHandlerFactory,
		IBearerTokenRetriever bearerTokenRetriever,
		IFileShareSelectorMenu fileShareSelectorMenu)
	{
		_consoleLogger = consoleLogger;
		_pathExtension = pathExtension;
		_relativityAuthenticationProviderFactory = relativityAuthenticationProviderFactory;
		_progressHandlerFactory = progressHandlerFactory;
		_bearerTokenRetriever = bearerTokenRetriever;
		_fileShareSelectorMenu = fileShareSelectorMenu;
	}

	public async Task ExecuteAsync(Configuration configuration, CancellationToken token)
    {
        var clientName = configuration.Common.ClientName;
        var jobId = configuration.Common.JobId;
        var source = new DirectoryPath(configuration.UploadDirectoryByWorkspaceId.Source);
        var authenticationProvider = _relativityAuthenticationProviderFactory.Create(configuration.Common);
        // This is transfer options object which is not necessary if you do not need change default parameters.
        var uploadDirectoryOptions = new UploadDirectoryOptions()
        {
            MaximumSpeed = default,
            OverwritePolicy = default,
            // ...
        };
        var progressHandler = _progressHandlerFactory.Create();

        // Get list of file shares by workspace ID. The association is based on Resource Pool assigned to the workspace.
        var fileSharesRetriever = new WorkspaceFileSharesRetriever(configuration, _bearerTokenRetriever, _pathExtension);
        var fileShareInfos = await fileSharesRetriever.GetWorkspaceFileSharesAsync().ConfigureAwait(false);
        var fileShareInfo = _fileShareSelectorMenu.SelectFileShare(fileShareInfos, token);

        if (fileShareInfo == null) return;

        // Build a destination path based on the selected file share (its UNC path).
        var destination = _pathExtension.GetDestinationDirectoryPathByFileShareInfo(fileShareInfo, configuration.Common.FileShareRelativePath, jobId);

        // The builder follows the Fluent convention, and more options will be added in the future. The only required component (besides the client name)
        // is the authentication provider - a provided one that utilizes an OAuth-based approach has been provided, but the custom implementation can be created.
        var transferClient = TransferClientBuilder.FullPathWorkflow
            .WithAuthentication(authenticationProvider)
            .WithClientName(clientName)
            .WithWorkspaceContext(configuration.UploadDirectoryByWorkspaceId.WorkspaceId)
            .Build();

        _consoleLogger.PrintCreatingTransfer(jobId, source, destination);

        var result = await transferClient
            .UploadDirectoryAsync(jobId, source, destination, uploadDirectoryOptions, progressHandler, token)
            // If you do not need pass transfer options you can invoke this method like this:
            //.UploadDirectoryAsync(jobId, source, destination, progressHandler, token)
            .ConfigureAwait(false);

        _consoleLogger.PrintTransferResult(result);
    }

    /// <summary>
    ///     Helper class to retrieve file shares by workspace ID.
    /// </summary>
    private class WorkspaceFileSharesRetriever
    {
        private const string GetFileShareServerUri =
            "/relativity.rest/api/Relativity.Services.Workspace.IWorkspaceModule/Workspace%20Manager%20Service/";

        private const string
            GetFileSharesMethodName =
                "GetAssociatedFileShareResourceServersAsync"; // this only reads OAuth2 Client data (which means the secret can be outdated)

        private const string MediaTypeApplicationJson = "application/json";
        private readonly Uri _baseUri;
        private readonly Configuration _configuration;
        private readonly IBearerTokenRetriever _bearerTokenRetriever1;
        private readonly IPathExtension _pathExtension;

        /// <summary>
        ///     Helper class to retrieve file shares by workspace ID.
        /// </summary>
        public WorkspaceFileSharesRetriever(Configuration configuration,
	        IBearerTokenRetriever bearerTokenRetriever,
	        IPathExtension pathExtension)
        {
	        _configuration = configuration;
	        _bearerTokenRetriever1 = bearerTokenRetriever;
	        _pathExtension = pathExtension;
	        _baseUri = new(new Uri(configuration.Common.InstanceUrl), GetFileShareServerUri);
        }


        /// <summary>
        ///     Retrieves list of file shares by workspace id.
        /// </summary>
        /// <returns>
        ///     List of file share items. To build a path, sample uses UNCPath property. Sample single item JSON:
        ///     <![CDATA[
        ///   {
        ///     "Status": {
        ///       "ArtifactID": 0,
        ///       "Guids": []
        ///     },
        ///     "UNCPath": "\\\\files.t025.r1.kcura.com\\T025\\Files\\",
        ///     "SystemCreatedOn": "0001-01-01T00:00:00",
        ///     "SystemLastModifiedOn": "0001-01-01T00:00:00",
        ///     "ArtifactID": 1014887,
        ///     "Name": "\\\\files.t025.r1.kcura.com\\T025\\Files\\",
        ///     "ServerType": {
        ///       "ArtifactID": 0
        ///     }
        ///   }
        /// ]]>
        /// </returns>
        public async Task<FileShareInfo[]> GetWorkspaceFileSharesAsync()
        {
            var token = await _bearerTokenRetriever1.RetrieveTokenAsync(new Uri(_configuration.Common.InstanceUrl),
                _configuration.Common.OAuthCredentials);

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-CSRF-Header", "-");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var endpoint = new Uri(_baseUri, GetFileSharesMethodName);
            var body = new StringContent(
                $"{{ workspace: {{ ArtifactId:'{_configuration.UploadDirectoryByWorkspaceId.WorkspaceId}'}}}}",
                Encoding.UTF8,
                MediaTypeApplicationJson);
            var response = await httpClient.PostAsync(endpoint, body);
            var fileShares =
                JsonConvert.DeserializeObject<List<ExpandoObject>>(await response.Content.ReadAsStringAsync());

            return fileShares.Select(x=> FileShareInfo.FromJson(x, _pathExtension)).ToArray();
        }
    }
}