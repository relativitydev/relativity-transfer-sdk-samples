using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByteSizeLib;
using Relativity.Transfer.SDK.Interfaces.Paths;
using Relativity.Transfer.SDK.Interfaces.ProgressReporting;
using Relativity.Transfer.SDK.Interfaces.ProgressReporting.JobStates;
using Relativity.Transfer.SDK.Samples.Core.Attributes;
using Relativity.Transfer.SDK.Samples.Core.DTOs;
using Spectre.Console;
using Rule = Spectre.Console.Rule;
using Color = Spectre.Console.Color;

namespace Relativity.Transfer.SDK.Samples.Core.Helpers;

internal sealed class ConsoleLogger : IConsoleLogger
{
	public Task PrintExitMessageAsync()
	{
		AnsiConsole.Clear();
		var rule = new Rule("[bold orange4]Application has been closed[/]")
		{
			Justification = Justify.Center,
			Style = new Style(Color.Orange4)
		};
		AnsiConsole.Write(rule);

		return Task.CompletedTask;
	}

	public void PrintCreatingTransfer(Guid jobId, PathBase source, PathBase destination,
		params string[] additionalLines)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine($"Creating transfer [green]{jobId}[/]:");
		if (source != null) AnsiConsole.MarkupLine($"\tFrom: [purple3]{source}[/]");
		if (destination != null) AnsiConsole.MarkupLine($"\tTo: [orange4]{destination}[/]");

		AddAdditionalLinesIfRequired(additionalLines);

		AnsiConsole.WriteLine();
	}

	public void PrintCreatingTransfer(Guid jobId, CloudLocation source, PathBase destination, params string[] additionalLines)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine($"Creating transfer [green]{jobId}[/]:");
		if (source != null) AnsiConsole.MarkupLine($"\tFrom: [purple3]{source}[/]");
		if (destination != null) AnsiConsole.MarkupLine($"\tTo: [orange4]{destination}[/]");

		AddAdditionalLinesIfRequired(additionalLines);

		AnsiConsole.WriteLine();
	}

	public void PrintTransferResult(TransferJobResult result, string headerLine = "Transfer has finished:",
		bool waitForKeyHit = true)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine(headerLine);
		AnsiConsole.MarkupLine($" JobId: [green]{result.CorrelationId}[/]");
		AnsiConsole.MarkupLine(
			$" Total Bytes: [green]{result.TotalBytes} ({ByteSize.FromBytes(result.TotalBytes)})[/]");
		AnsiConsole.MarkupLine($" Total Files Transferred: [green]{result.TotalFilesTransferred}[/]");
		AnsiConsole.MarkupLine(
			$" Total Empty Directories Transferred: [green]{result.TotalEmptyDirectoriesTransferred}[/]");
		AnsiConsole.MarkupLine($" Total Files Skipped: [yellow]{result.TotalFilesSkipped}[/]");
		AnsiConsole.MarkupLine($" Total Files Failed: [red]{result.TotalFilesFailed}[/]");
		AnsiConsole.MarkupLine($" Total Empty Directories Failed: [red]{result.TotalEmptyDirectoriesFailed}[/]");
		AnsiConsole.MarkupLine(
			$" Elapsed: [green]{result.Elapsed:hh\\:mm\\:ss} s ({Math.Floor(result.Elapsed.TotalSeconds)} s)[/]");
		AnsiConsole.MarkupLine($" State: [red]{GetStatusMarkup(result)}[/]");
		AnsiConsole.WriteLine();

		AddProgressStepsDescription(result);

		if (!waitForKeyHit) return;

		AnsiConsole.Markup("Press any key to continue...");
		Console.ReadKey();
	}

	public void Info(string msg)
	{
		AnsiConsole.MarkupLine(msg);
	}

	public void PrintError(Exception exception)
	{
		AnsiConsole.WriteException(exception);
		AnsiConsole.WriteLine();
		AnsiConsole.Markup("Press any key to continue...");
		Console.ReadKey();
	}

	public SampleAttribute PrintMainMenu()
	{
		AnsiConsole.Clear();
		var prompt = new SelectionPrompt<SampleAttribute>()
			.Title("[green]Use arrow up and down to select a sample to run:[/]")
			.PageSize(15)
			.MoreChoicesText("[grey](Move up and down to reveal more samples)[/]")
			.AddChoices(SamplesAttributesProvider.GetSamplesAttributes());
		var selectedAttribute = AnsiConsole.Prompt(prompt);

		return selectedAttribute?.IsExitOption ?? true
			? null
			: selectedAttribute;
	}

	public FileShareInfo PrintFileShareInfosMenu(IEnumerable<FileShareInfo> fileShareInfos)
	{
		AnsiConsole.WriteLine();
		var prompt = new SelectionPrompt<FileShareInfo>()
			.Title("[green]Use arrow up and down to select a file share to use:[/]")
			.PageSize(15)
			.MoreChoicesText("[grey](Move up and down to reveal more file shares)[/]")
			.AddChoices(fileShareInfos.OrderBy(x => x.Name).Concat(new[] { FileShareInfo.BackToToMainMenu }));
		var selectedFileShare = AnsiConsole.Prompt(prompt);

		return selectedFileShare?.IsBackToMainMenuOption ?? true
			? null
			: selectedFileShare;
	}

	public void PrintRegisteringTransferJob(Guid jobId, PathBase source = null, PathBase destination = null,
		Guid? existingJobId = null)
	{
		AnsiConsole.WriteLine();
		if (source != null)
			Info(
				$"[orange4]Registering the transfer job[/] [green]{jobId}[/] [orange4]for the source path[/] [green]{source}[/]");
		if (destination != null)
			Info(
				$"[orange4]Registering the transfer job[/] [green]{jobId}[/] [orange4]for the destination path[/] [green]{destination}[/]");
		if (existingJobId.HasValue)
			Info(
				$"[orange4]Registering the transfer job[/] [green]{jobId}[/] [orange4]with destination taken from the existing transfer job[/] [green]{existingJobId}[/]");
		AnsiConsole.WriteLine();
	}

	private static void AddAdditionalLinesIfRequired(string[] additionalLines)
	{
		var lines = (additionalLines ?? Array.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
		if (lines.Length == 0) return;

		AnsiConsole.WriteLine();
		foreach (var additionalLine in lines)
			AnsiConsole.MarkupLine(additionalLine);
	}

	private static void AddProgressStepsDescription(TransferJobResult result)
	{
		if (!result.ProgressSteps.Any()) return;

		AnsiConsole.MarkupLine("  Job's steps:");
		foreach (var stepProgress in result.ProgressSteps)
			AnsiConsole.MarkupLine(
				$"    Step [orange4]{stepProgress.StepType}[/]: [green]{stepProgress.PercentageProgress:F}%[/], {GetStatusMarkup(stepProgress.State)}");
		AnsiConsole.WriteLine();
	}

	private static string GetStatusMarkup(TransferJobResult result)
	{
		var color = result.State switch
		{
			TransferJobCancelled => "yellow",
			TransferJobFailed => "red",
			TransferJobCompleted => "green",
			TransferJobCompletedWithWarnings => "orange4",
			_ => "grey46"
		};

		return $"[{color}]{result.State.Status}[/]";
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