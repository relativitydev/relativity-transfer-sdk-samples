using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;
using Relativity.Transfer.SDK.Core.ProgressReporting;
using Relativity.Transfer.SDK.Interfaces.Options;
using Relativity.Transfer.SDK.Interfaces.Paths;
using Relativity.Transfer.SDK.Interfaces.ProgressReporting;
using Relativity.Transfer.SDK.Samples.Core.Attributes;
using Relativity.Transfer.SDK.Samples.Core.Authentication;
using Relativity.Transfer.SDK.Samples.Core.Configuration;
using Relativity.Transfer.SDK.Samples.Core.Helpers;
using Relativity.Transfer.SDK.Samples.Core.Runner;

namespace Relativity.Transfer.SDK.Samples.Repository.FullPathWorkflow;

[Sample(SampleOrder.SettingUpProgressHandlerAndPrintingSummary, "A progress handler",
    "The sample illustrates how to implement a progress handler (statistics and events for files).",
    typeof(SettingUpProgressHandlerAndPrintingSummary),
    TransferType.UploadDirectory)]
internal class SettingUpProgressHandlerAndPrintingSummary : ISample
{
	private readonly IConsoleLogger _consoleLogger;
	private readonly IPathExtension _pathExtension;
	private readonly IRelativityAuthenticationProviderFactory _relativityAuthenticationProviderFactory;

	public SettingUpProgressHandlerAndPrintingSummary(IConsoleLogger consoleLogger,
		IPathExtension pathExtension,
		IRelativityAuthenticationProviderFactory relativityAuthenticationProviderFactory)
	{
		_consoleLogger = consoleLogger;
		_pathExtension = pathExtension;
		_relativityAuthenticationProviderFactory = relativityAuthenticationProviderFactory;
	}

	public async Task ExecuteAsync(Configuration configuration, CancellationToken token)
    {
        var clientName = configuration.Common.ClientName;
        var jobId = configuration.Common.JobId;
        var source = string.IsNullOrWhiteSpace(configuration.UploadDirectory.Source)
            ? _pathExtension.CreateTemporaryDirectoryWithFiles(jobId)
            : new DirectoryPath(configuration.UploadDirectory.Source);
        var additionalLine = string.IsNullOrWhiteSpace(configuration.UploadDirectory.Source)
            ? "This Sample will display statistics while transferring a few small files, a few bytes each, and a few large files, each with a size of a few hundreds MiB"
            : string.Empty;
        var destination = string.IsNullOrWhiteSpace(configuration.UploadDirectory.Destination)
            ? _pathExtension.GetDefaultRemoteDirectoryPathForUpload(configuration.Common)
            : new DirectoryPath(configuration.UploadDirectory.Destination);
        // This is transfer options object which is not necessary if you do not need change default parameters.
        var uploadDirectoryOptions = new UploadDirectoryOptions()
        {
            MaximumSpeed = default,
            OverwritePolicy = default,
            // ...
        };
        var authenticationProvider = _relativityAuthenticationProviderFactory.Create(configuration.Common);

        // The builder follows the Fluent convention, and more options will be added in the future. The only required component (besides the client name)
        // is the authentication provider - a provided one that utilizes an OAuth-based approach has been provided, but the custom implementation can be created.
        var transferClient = TransferClientBuilder.FullPathWorkflow
            .WithAuthentication(authenticationProvider)
            .WithClientName(clientName)
            .WithStagingExplorerContext()
            .Build();

        _consoleLogger.PrintCreatingTransfer(jobId, source, destination, additionalLine);

        var result = await transferClient
            .UploadDirectoryAsync(jobId, source, destination, uploadDirectoryOptions, GetProgressHandler(), default)
            // If you do not need pass transfer options you can invoke this method like this:
            //.UploadDirectoryAsync(jobId, source, destination, GetProgressHandler(), default)
            .ConfigureAwait(false);

        PrintTransferSummary(result);
    }

    // All below methods provide a sample implementation of the Relativity.Transfer.SDK.Interfaces.ProgressReporting.ITransferProgressHandler interface. These methods can be used as a starting point for your own implementation.
    // The Console class is used to print the progress to the console, but you can utilize any other logging mechanism (keep in mind that long-running operations should be avoided because they are blocking operations).

    private static ITransferProgressHandler GetProgressHandler()
    {
        // Transfer progress is optional - you can subscribe to all, some, or no updates. Bear in mind though that this is the only way you can obtain information
        // about eventual fails or skips on individual items and that's why encourage the clients to take an advantage of it (subscribe and log it).
        // TransferSDK doesn't store any information about failed file paths due to privacy concerns.
        return TransferProgressHandlerBuilder.Instance
            .OnStatistics(
                PrintStatistics) // Updates about overall status (% progress, transfer rates, partial statistics)
            .OnSucceededItem(PrintSucceededItem) // Updates on each transferred item
            .OnFailedItem(PrintFailedItem) // Updates on each failed item (and reason for it)
            .OnSkippedItem(PrintSkippedItem) // Updates on each skipped item (and reason for it)
            .OnProgressSteps(
                PrintProgressStep) // Updates on each job's progress steps. Use it to track overall percentage progress and state.
            .Create();
    }

    private static void PrintStatistics(TransferJobStatistics statistics)
    {
        WriteLine($"  bytes transferred: {statistics.CurrentBytesTransferred} of {statistics.TotalBytes}",
            ConsoleColor.Blue);
    }

    private static void PrintSucceededItem(TransferItemState itemState)
    {
        WriteLine($"  item transfer succeeded: {itemState.Source}", ConsoleColor.DarkGreen);
    }

    private static void PrintFailedItem(TransferItemState itemState)
    {
        WriteLine($"  item transfer failed: {itemState.Source}", ConsoleColor.Red);
    }

    private static void PrintSkippedItem(TransferItemState itemState)
    {
        WriteLine($"  item transfer skipped: {itemState.Source}", ConsoleColor.Yellow);
    }

    private static void PrintProgressStep(IEnumerable<StepProgress> progressSteps)
    {
        WriteLine(
            string.Join(Environment.NewLine,
                progressSteps.Select(stepProgress =>
                    $"    step name: {stepProgress.StepType}, {stepProgress.PercentageProgress:F}%, state: {stepProgress.State}")),
            ConsoleColor.Cyan);
    }

    private static void PrintTransferSummary(TransferJobResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("Transfer has finished: ");
        sb.AppendLine($"  - JobId: {result.CorrelationId}");
        sb.AppendLine($"  - Total Bytes: {result.TotalBytes} ({ByteSize.FromBytes(result.TotalBytes)})");
        sb.AppendLine($"  - Total Files Transferred: {result.TotalFilesTransferred}");
        sb.AppendLine($"  - Total Empty Directories Transferred: {result.TotalEmptyDirectoriesTransferred}");
        sb.AppendLine($"  - Total Files Skipped: {result.TotalFilesSkipped}");
        sb.AppendLine($"  - Total Files Failed: {result.TotalFilesFailed}");
        sb.AppendLine($"  - Total Empty Directories Failed: {result.TotalEmptyDirectoriesFailed}");
        sb.AppendLine($"  - Elapsed: {result.Elapsed:hh\\:mm\\:ss} s ({Math.Floor(result.Elapsed.TotalSeconds)} s)");
        sb.AppendLine($"  - Status: {result.State.Status}");
        sb.AppendLine();
        sb.AppendLine("Press any key to continue...");

        WriteLine(sb.ToString(), ConsoleColor.Blue);
        Console.ReadKey();
    }

    private static void WriteLine(string line, ConsoleColor color)
    {
        var defaultColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(line);
        Console.ForegroundColor = defaultColor;
    }
}