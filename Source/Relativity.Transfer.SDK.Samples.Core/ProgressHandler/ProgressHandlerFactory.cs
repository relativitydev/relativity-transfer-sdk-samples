using System.Collections.Generic;
using System.Linq;
using ByteSizeLib;
using Relativity.Transfer.SDK.Core.ProgressReporting;
using Relativity.Transfer.SDK.Interfaces.ProgressReporting;
using Spectre.Console;

namespace Relativity.Transfer.SDK.Samples.Core.ProgressHandler;

internal sealed class ProgressHandlerFactory : IProgressHandlerFactory
{
	public ITransferProgressHandler Create()
	{
		// Transfer progress is optional - you can subscribe to all, some, or no updates. Bear in mind though that this is the only way you can obtain information
		// about eventual fails or skips on individual items and that's why encourage the clients to take an advantage of it (subscribe and log it).
		// TransferSDK doesn't store any information about failed file paths due to privacy concerns.
		return TransferProgressHandlerBuilder.Instance
			.OnStatistics(
				OnStatisticsReceived) // Updates about overall status (% progress, transfer rates, partial statistics)
			.OnSucceededItem(OnSucceededItemReceived) // Updates on each transferred item
			.OnFailedItem(OnFailedItemReceived) // Updates on each failed item (and reason for it)
			.OnSkippedItem(OnSkippedItemReceived) // Updates on each skipped item (and reason for it)
			.OnProgressSteps(
				OnProgressStepsReceived) // Updates on each job's progress steps. Use it to track overall percentage progress and state.
			.Create();
	}

	private static void OnStatisticsReceived(TransferJobStatistics statistics)
	{
		AnsiConsole.MarkupLine(BuildStatisticsMessage(statistics));
	}

	private static void OnSucceededItemReceived(TransferItemState update)
	{
		AnsiConsole.MarkupLine(
			$" [green]{update.Timestamp:T}[/]: Element transferred. Source: [green]{update.Source}[/] Destination: [green]{update.Destination}[/]");
	}

	private static void OnSkippedItemReceived(TransferItemState update)
	{
		AnsiConsole.MarkupLine(
			$" [yellow]{update.Timestamp:T}[/]: Element skipped. Source: [yellow]{update.Source}[/] Destination: [yellow]{update.Destination}[/] Error: [red]{update.Exception?.Message}[/]");
	}

	private static void OnFailedItemReceived(TransferItemState update)
	{
		AnsiConsole.MarkupLine(
			$" [red]{update.Timestamp:T}[/]: Element failed. Source: [red]{update.Source}[/] Error: [red]{update.Exception?.Message}[/]");
	}

	private static void OnProgressStepsReceived(IEnumerable<StepProgress> progressSteps)
	{
		var stepProgresses = progressSteps as StepProgress[] ?? progressSteps.ToArray();
		if (!stepProgresses.Any()) return;

		foreach (var stepProgress in stepProgresses)
			AnsiConsole.MarkupLine(
				$"  Step name: [orange4]{stepProgress.StepType}[/],  [green]{stepProgress.PercentageProgress:F}%[/], State: {GetStatusMarkup(stepProgress.State)}");
	}

	private static string BuildStatisticsMessage(TransferJobStatistics statistics)
	{
		const string notKnown = "-";
		var totalMegaBytes = statistics.TotalBytes.HasValue
			? $"{ByteSize.FromBytes((double)statistics.TotalBytes).MegaBytes:F}"
			: notKnown;
		var bytesStatistic = $"{ByteSize.FromBytes(statistics.CurrentBytesTransferred).MegaBytes:F}/{totalMegaBytes}";

		var totalItems = statistics.TotalItems.HasValue ? statistics.TotalItems.ToString() : notKnown;
		var transferredItems = $"{statistics.CurrentItemsTransferred}/{totalItems}";

		var estimatedTime = statistics.EstimatedTime.HasValue
			? statistics.EstimatedTime.Value.ToString(@"h\:mm\:ss")
			: notKnown;

		return
			$"[green]{statistics.Timestamp:T}[/]: [green]{bytesStatistic} MB[/], Items: [green]{transferredItems}[/], Skipped: [yellow]{statistics.CurrentItemsSkipped}[/], Failed: [red]{statistics.CurrentItemsFailed}[/], ETA: [orange4]{estimatedTime}[/]";
	}

	private static string GetStatusMarkup(TransferStepState state)
	{
		var color = state switch
		{
			TransferStepState.Canceled => "yellow",
			TransferStepState.Failed => "red",
			TransferStepState.Fatal => "red",
			TransferStepState.Completed => "green",
			TransferStepState.CompletedWithWarnings => "orange4",
			_ => "grey46"
		};

		return $"[{color}]{state}[/]";
	}
}