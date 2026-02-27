using System;
using Relativity.Transfer.SDK.Samples.Core.Attributes;
using Relativity.Transfer.SDK.Samples.Core.Authentication.Credentials;
using Relativity.Transfer.SDK.Samples.Core.Configuration;
using Relativity.Transfer.SDK.Samples.Core.Helpers;
using Spectre.Console;
using Rule = Spectre.Console.Rule;
using Color = Spectre.Console.Color;

namespace Relativity.Transfer.SDK.Samples.Core.UI;

internal sealed class ConfigurationScreen : IConfigurationScreen
{
	private readonly IPathExtension _pathExtension;

	public ConfigurationScreen(IPathExtension pathExtension)
	{
		_pathExtension = pathExtension;
	}

	public Configuration.Configuration UpdateConfiguration(Configuration.Configuration configuration,
		SampleAttribute sampleAttribute)
	{
		if (sampleAttribute.TransferType == TransferType.Default)
			throw new ArgumentOutOfRangeException(nameof(sampleAttribute.TransferType));

		PrintTitle(sampleAttribute);
		PrintDescription(sampleAttribute);

		return AskForParameters(configuration, sampleAttribute);
	}

	private Configuration.Configuration AskForParameters(Configuration.Configuration configuration,
		SampleAttribute sampleAttribute)
	{
		PrintTitle("Sample configuration (press enter to use default value)");

		var newCommon = sampleAttribute.TransferType == TransferType.CloudUpload ? AskForCloudUploadParameters(configuration) : AskForCommonParameters(configuration);

		return AskForTransferTypeSpecificParameters(configuration, sampleAttribute, newCommon);
	}

	private Configuration.Configuration AskForTransferTypeSpecificParameters(
		Configuration.Configuration configuration,
		SampleAttribute sampleAttribute, CommonConfiguration common)
	{
		string source, destination;
		switch (sampleAttribute.TransferType)
		{
			case TransferType.UploadDirectory:
				source = AnsiConsole.Ask("Source directory", configuration.UploadDirectory.Source);
				destination = AnsiConsole.Ask("Destination directory",
					GetDefaultUploadDirectoryDestination(common, configuration.UploadDirectory.Destination));

				return Configuration.Configuration.ForUploadDirectory(common,
					new SourceAndDestinationConfiguration(source, destination));
			
			case TransferType.CloudUpload:
				source = AnsiConsole.Ask("Source SasURL", configuration.CloudUpload.Source);
				destination = AnsiConsole.Ask("Destination directory", GetDefaultUploadDirectoryDestination(common, configuration.CloudUpload.Destination));
				
				return Configuration.Configuration.ForCloudUpload(common, new SourceAndDestinationConfiguration(source, destination));

			case TransferType.UploadFile:
				source = AnsiConsole.Ask("Source file", configuration.UploadFile.Source); 
				destination = AnsiConsole.Ask("Destination directory", configuration.UploadFile.Destination);

				return Configuration.Configuration.ForUploadFile(common,
					new SourceAndDestinationConfiguration(source, destination));
			
			case TransferType.UploadItems:
				source = AnsiConsole.Ask("List of items source file (load file)", configuration.UploadFile.Source);
				destination = AnsiConsole.Ask("Destination root directory",
					GetDefaultUploadDirectoryDestination(common, configuration.UploadDirectory.Destination));
				
				return Configuration.Configuration.ForUploadItems(common,
					new SourceAndDestinationConfiguration(source, destination));

			case TransferType.DownloadDirectory:
				source = AnsiConsole.Ask("Source directory", GetDefaultDownloadDirectorySource(configuration, common));
				destination = AnsiConsole.Ask("Destination directory", configuration.DownloadDirectory.Destination);

				return Configuration.Configuration.ForDownloadDirectory(common,
					new SourceAndDestinationConfiguration(source, destination));

			case TransferType.DownloadFile:
				source = AnsiConsole.Ask("Source file", configuration.DownloadFile.Source);
				destination = AnsiConsole.Ask("Destination directory", configuration.DownloadFile.Destination);

				return Configuration.Configuration.ForDownloadFile(common,
					new SourceAndDestinationConfiguration(source, destination));

			case TransferType.UploadDirectoryByWorkspaceId:
				source = AnsiConsole.Ask("Source directory", configuration.UploadDirectoryByWorkspaceId.Source);
				var workspaceId = AnsiConsole.Prompt(
					new TextPrompt<int>("Workspace ID to list its file shares")
						.DefaultValue(configuration.UploadDirectoryByWorkspaceId.WorkspaceId)
						.ValidationErrorMessage("[red]Invalid workspace ID (numeric only values are allowed)[/]")
						.Validate(id =>
						{
							return id switch
							{
								< -1 => ValidationResult.Error("[red]Workspace ID can not be lower than -1[/]"),
								_ => ValidationResult.Success()
							};
						}));

				return Configuration.Configuration.ForUploadDirectoryByWorkspaceId(common,
					new SourceAndWorkspaceIdConfiguration(source, workspaceId));

			case TransferType.UploadDirectoryBasedOnExistingJob:
				// Because of the issue in SpectreConsole, validation needs to be made manually: https://github.com/spectreconsole/spectre.console/issues/1343
				var firstSource = string.Empty;
				var secondSource = string.Empty;
				var isValid = false;
				while (!isValid)
				{
					firstSource = AnsiConsole.Ask("First source directory",
						configuration.UploadDirectoryBasedOnExistingJob.FirstSource);
					secondSource = AnsiConsole.Ask("Second source directory",
						configuration.UploadDirectoryBasedOnExistingJob.SecondSource);
					isValid = firstSource != secondSource;
					if (!isValid)
						AnsiConsole.MarkupLine(
							"[red]The second source path should be different than the first source path[/]");
				}

				destination = AnsiConsole.Ask("Destination directory",
					GetDefaultUploadDirectoryDestination(common,
						configuration.UploadDirectoryBasedOnExistingJob.Destination));

				return Configuration.Configuration.ForUploadDirectoryBasedOnExistingJob(common,
					new TwoSourcesAndDestinationConfiguration(firstSource, secondSource, destination));

			case TransferType.DownloadDirectoryBasedOnExistingJob:
				source = AnsiConsole.Ask("Source directory", configuration.DownloadDirectoryBasedOnExistingJob.Source);
				var firstDestination = AnsiConsole.Ask("First destination directory [[upload]]",
					GetDefaultUploadDirectoryDestination(common,
						configuration.DownloadDirectoryBasedOnExistingJob.FirstDestination));
				var secondDestination = AnsiConsole.Ask("Second destination directory [[download]]",
					configuration.DownloadDirectoryBasedOnExistingJob.SecondDestination);

				return Configuration.Configuration.ForDownloadDirectoryBasedOnExistingJob(common,
					new SourceAndTwoDestinationsConfiguration(source, firstDestination, secondDestination));

			default: throw new ArgumentOutOfRangeException(nameof(sampleAttribute.TransferType));
		}
	}

	private string GetDefaultUploadDirectoryDestination(CommonConfiguration common, string defaultValue)
	{
		return string.IsNullOrWhiteSpace(defaultValue)
			? _pathExtension.GetDefaultRemoteDirectoryPathForUploadAsString(common)
			: defaultValue;
	}

	private string GetDefaultDownloadDirectorySource(Configuration.Configuration configuration,
		CommonConfiguration common)
	{
		var source = configuration.DownloadDirectory.Source;

		return string.IsNullOrWhiteSpace(source)
			? _pathExtension.GetDefaultRemoteDirectoryPathForDownloadAsString(common)
			: source;
	}

	private static CommonConfiguration AskForCommonParameters(Configuration.Configuration configuration)
	{
		AnsiConsole.MarkupLine($"Client name [green]({configuration.Common.ClientName})[/]");
		var jobId = Guid.NewGuid();
		AnsiConsole.MarkupLine($"Job ID [green]({jobId})[/]");
		var instanceUrl = AnsiConsole.Ask("Instance URL", configuration.Common.InstanceUrl);
		var fileShareRoot = AnsiConsole.Ask("File share root", configuration.Common.FileShareRoot);
		var fileShareRelativePath =
			AnsiConsole.Ask("File share relative path", configuration.Common.FileShareRelativePath);
		var clientId = AnsiConsole.Ask("Client secret ID", configuration.Common.OAuthCredentials.ClientId);
		var clientSecret = AnsiConsole.Ask("Client secret", configuration.Common.OAuthCredentials.ClientSecret);

		return new CommonConfiguration(configuration.Common.ClientName, instanceUrl, fileShareRoot,
			fileShareRelativePath, new OAuthCredentials(clientId, clientSecret))
		{
			JobId = jobId
		};
	}
	
	private static CommonConfiguration AskForCloudUploadParameters(Configuration.Configuration configuration)
	{
		AnsiConsole.MarkupLine($"Client name [green]({configuration.Common.ClientName})[/]");
		var jobId = Guid.NewGuid();
		AnsiConsole.MarkupLine($"Job ID [green]({jobId})[/]");
		var instanceUrl = AnsiConsole.Ask("Instance URL", configuration.Common.InstanceUrl);
		var clientId = AnsiConsole.Ask("Client secret ID", configuration.Common.OAuthCredentials.ClientId);
		var clientSecret = AnsiConsole.Ask("Client secret", configuration.Common.OAuthCredentials.ClientSecret);

		return new CommonConfiguration(configuration.Common.ClientName, instanceUrl, string.Empty,
			string.Empty, new OAuthCredentials(clientId, clientSecret))
		{
			JobId = jobId
		};
	}

	private static void PrintDescription(SampleAttribute sampleAttribute)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.Markup($"[green]{sampleAttribute.SampleDescription}[/]");
		AnsiConsole.WriteLine();
	}

	private static void PrintTitle(SampleAttribute sampleAttribute)
	{
		AnsiConsole.Clear();
		var rule = new Rule($"[bold orange4]{sampleAttribute.MenuCaption}[/]")
		{
			Justification = Justify.Center,
			Style = new Style(Color.Orange4)
		};
		AnsiConsole.Write(rule);
	}

	private static void PrintTitle(string title)
	{
		AnsiConsole.WriteLine();
		var rule = new Rule($"[bold orange4]{title}[/]")
		{
			Justification = Justify.Center,
			Style = new Style(Color.Orange4)
		};
		AnsiConsole.Write(rule);
		AnsiConsole.WriteLine();
	}
}