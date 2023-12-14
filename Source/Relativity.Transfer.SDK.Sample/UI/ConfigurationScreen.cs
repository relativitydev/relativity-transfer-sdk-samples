﻿using System;
using Relativity.Transfer.SDK.Sample.Attributes;
using Relativity.Transfer.SDK.Sample.Authentication.Credentials;
using Relativity.Transfer.SDK.Sample.Configuration;
using Relativity.Transfer.SDK.Sample.Helpers;
using Spectre.Console;

namespace Relativity.Transfer.SDK.Sample.UI;

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
		var newCommon = AskForCommonParameters(configuration);

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
					GetDefaultUploadDirectoryDestination(configuration, common));

				return Configuration.Configuration.ForUploadDirectory(common,
					new SourceAndDestinationConfiguration(source, destination));
			case TransferType.UploadFile:
				source = AnsiConsole.Ask("Source file", configuration.UploadFile.Source);
				destination = AnsiConsole.Ask("Destination directory", configuration.UploadFile.Destination);

				return Configuration.Configuration.ForUploadFile(common,
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
				source = AnsiConsole.Ask("Source directory", configuration.UploadDirectory.Source);
				var workspaceId = AnsiConsole.Prompt(
					new TextPrompt<int>("Workspace ID to list its file shares")
						.DefaultValue(configuration.UploadDirectoryByWorkspaceId.WorkspaceId)
						.ValidationErrorMessage("[red]Invalid workspace ID (numeric only values are allowed)[/]")
						.Validate(id =>
						{
							return id switch
							{
								< -1 => ValidationResult.Error("[red]Workspace ID can not be lower than -1[/]"),
								_ => ValidationResult.Success(),
							};
						}));

				return Configuration.Configuration.ForUploadDirectoryByWorkspaceId(common,
					new SourceAndWorkspaceIdConfiguration(source, workspaceId));

			default: throw new ArgumentOutOfRangeException(nameof(sampleAttribute.TransferType));
		}
	}

	private string GetDefaultUploadDirectoryDestination(Configuration.Configuration configuration,
		CommonConfiguration common)
	{
		var destination = configuration.UploadDirectory.Destination;

		return string.IsNullOrWhiteSpace(destination)
			? _pathExtension.GetDefaultRemoteDirectoryPathForUploadAsString(common)
			: destination;
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