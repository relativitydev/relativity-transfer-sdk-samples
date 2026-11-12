using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Transfer.SDK.Interfaces.Options;
using Relativity.Transfer.SDK.Interfaces.Paths;
using Relativity.Transfer.SDK.Interfaces.ProgressReporting.JobStates;
using Relativity.Transfer.SDK.Samples.Core.Attributes;
using Relativity.Transfer.SDK.Samples.Core.Authentication;
using Relativity.Transfer.SDK.Samples.Core.Configuration;
using Relativity.Transfer.SDK.Samples.Core.Helpers;
using Relativity.Transfer.SDK.Samples.Core.ProgressHandler;
using Relativity.Transfer.SDK.Samples.Core.Runner;
using Spectre.Console;

namespace Relativity.Transfer.SDK.Samples.Repository.FullPathWorkflow;

// R1-to-R1 (RelativityOne-to-RelativityOne) transfer copies a directory from one RelativityOne instance to another.
// The data is copied server-side, no customer machine is involved.
//
// There are two parties, each working ONLY with its own instance and its own OAuth2 client:
//  - RECEIVER - owns the DESTINATION instance. Creates the reference token (step 1), shares it (step 2) and optionally revokes it (step 5).
//  - SENDER   - owns the SOURCE instance. Validates the token (step 3) and runs the transfer (step 4).
//
// This sample plays both roles in a single process, so it needs credentials to both instances.
// Workflow: https://platform.relativity.com/RelativityaiR/Content/Transfer_API/Relativity_aiR-to-Relativity_aiR_transfers.htm#workflow
[Sample(SampleOrder.R1ToR1EndToEnd,
	"R1-to-R1 transfer (end-to-end)",
	"The sample walks step by step through the whole R1-to-R1 workflow: the Receiver creates a reference token on the destination instance, " +
	"the Sender validates it and transfers a directory from the source instance, and the Receiver revokes the token if the transfer failed or was cancelled.",
	typeof(R1ToR1EndToEnd),
	TransferType.R1ToR1EndToEnd)]
internal class R1ToR1EndToEnd : ISample
{
	private readonly IConsoleLogger _consoleLogger;
	private readonly IRelativityAuthenticationProviderFactory _relativityAuthenticationProviderFactory;
	private readonly IProgressHandlerFactory _progressHandlerFactory;

	public R1ToR1EndToEnd(IConsoleLogger consoleLogger,
		IRelativityAuthenticationProviderFactory relativityAuthenticationProviderFactory,
		IProgressHandlerFactory progressHandlerFactory)
	{
		_consoleLogger = consoleLogger;
		_relativityAuthenticationProviderFactory = relativityAuthenticationProviderFactory;
		_progressHandlerFactory = progressHandlerFactory;
	}

	public async Task ExecuteAsync(Configuration configuration, CancellationToken token)
	{
		var clientName = configuration.Common.ClientName;
		var jobId = configuration.Common.JobId;
		var r1ToR1 = configuration.R1ToR1;
		var source = new DirectoryPath(r1ToR1.Source);

		// =============================================================================================
		// RECEIVER (destination instance) - setup.
		// The Receiver authenticates against the DESTINATION instance (R1ToR1 section of the configuration).
		// Reference tokens are managed by a dedicated client built with TransferClientBuilder.ReferenceTokenWorkflow.
		// =============================================================================================
		var destinationAuthenticationProvider = _relativityAuthenticationProviderFactory.Create(configuration.Common with
		{
			InstanceUrl = r1ToR1.DestinationInstanceUrl,
			OAuthCredentials = r1ToR1.DestinationOAuthCredentials
		});
		var receiverTokenClient = TransferClientBuilder.ReferenceTokenWorkflow
			.WithAuthentication(destinationAuthenticationProvider)
			.Build();

		// =============================================================================================
		// SENDER (source instance) - setup.
		// The Sender authenticates against the SOURCE instance (Common section of the configuration).
		// It never needs credentials to the destination instance - the reference token is all it gets from the Receiver.
		// =============================================================================================
		var sourceAuthenticationProvider = _relativityAuthenticationProviderFactory.Create(configuration.Common);

		// ---------------------------------------------------------------------------------------------
		// Step 1 - RECEIVER (destination instance): create a reference token for the destination directory.
		// The directory must be in a staging area and must already exist. The token wraps a SAS token valid for 7 days,
		// the SAS token itself is never exposed to either party.
		// ---------------------------------------------------------------------------------------------
		var referenceToken = await receiverTokenClient.CreateAsync(r1ToR1.Destination, token).ConfigureAwait(false);

		// ---------------------------------------------------------------------------------------------
		// Step 2 - RECEIVER -> SENDER: share the token through a secure out-of-band channel.
		// In this sample both roles live in one process, so the token is simply passed along in memory.
		// ---------------------------------------------------------------------------------------------

		// ---------------------------------------------------------------------------------------------
		// Step 3 - SENDER (source instance): validate the token to confirm the destination before sending any data.
		// The result tells who issued the token (CustomerName), so the Sender can make sure the data goes to the expected party.
		// ---------------------------------------------------------------------------------------------
		var senderTokenClient = TransferClientBuilder.ReferenceTokenWorkflow
			.WithAuthentication(sourceAuthenticationProvider)
			.Build();
		var validationResult = await senderTokenClient.ValidateAsync(referenceToken, token).ConfigureAwait(false);
		if (!validationResult.IsValid)
			throw new InvalidOperationException($"Reference token is not valid: {validationResult.ErrorMessage}");

		// ---------------------------------------------------------------------------------------------
		// Step 4 - SENDER (source instance): create the transfer job and monitor it.
		// The documented REST workflow has two calls here (POST jobs + polling GET jobs/{id}). The SDK handles both in a single call:
		// it creates the job and polls its status until it finishes, reporting progress through the progress handler.
		// R1-to-R1 transfer uses the same full path client as the other transfers.
		// ---------------------------------------------------------------------------------------------
		var transferClient = TransferClientBuilder.FullPathWorkflow
			.WithAuthentication(sourceAuthenticationProvider)
			.WithClientName(clientName)
			.WithStagingExplorerContext()
			.Build();

		_consoleLogger.PrintCreatingTransfer(jobId, source, null,
			$"\tTo: [orange4]{Markup.Escape(r1ToR1.Destination)}[/] (encoded in the reference token)");

		var options = new R1ToR1TransferOptions
		{
			// Cancelling this token cancels the job on the server (the SDK calls the cancel endpoint and waits until
			// the job is cancelled). The method-level 'token' only stops the SDK-side processing.
			JobCancellationToken = CancellationToken.None
		};
		var result = await transferClient
			.R1ToR1TransferAsync(jobId, referenceToken, source, options, _progressHandlerFactory.Create(), token)
			.ConfigureAwait(false);

		// ---------------------------------------------------------------------------------------------
		// Step 5 (optional) - RECEIVER (destination instance): revoke the token. Only the Receiver can revoke it.
		// - Transfer failed or was cancelled: the token stays active - revoke it so it can not be used again.
		// - Transfer completed: do NOT revoke it. The token is revoked automatically once the post-transfer QC finishes.
		//   Note: R1ToR1TransferAsync returns when the transfer finishes, BEFORE the QC is done. The SDK does not expose
		//   the QC status yet (a dedicated client is coming soon) - for now check it via the REST API:
		//   GET {source_instance_url}/transfer-service/api/v2/transfer/jobs/{jobId}/qc
		//   https://platform.relativity.com/RelativityaiR/Content/Transfer_API/Relativity_Transfer_Service.htm#GETv2transferjobsjobIdqc
		// ---------------------------------------------------------------------------------------------
		if (result.State is TransferJobFailed or TransferJobCancelled)
			await receiverTokenClient.RevokeAsync(referenceToken, token).ConfigureAwait(false);

		_consoleLogger.PrintTransferResult(result);
	}
}
