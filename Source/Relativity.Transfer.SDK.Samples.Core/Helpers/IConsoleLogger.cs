using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Transfer.SDK.Interfaces.Paths;
using Relativity.Transfer.SDK.Interfaces.ProgressReporting;
using Relativity.Transfer.SDK.Samples.Core.Attributes;
using Relativity.Transfer.SDK.Samples.Core.DTOs;

namespace Relativity.Transfer.SDK.Samples.Core.Helpers;

internal interface IConsoleLogger
{
	Task PrintExitMessageAsync();
	void PrintCreatingTransfer(Guid jobId, PathBase source, PathBase destination, params string[] additionalLines);
	void PrintCreatingTransfer(Guid jobId, CloudLocation source, PathBase destination, params string[] additionalLines);
	void PrintTransferResult(TransferJobResult result, string headerLine = "Transfer has finished:", bool waitForKeyHit = true);
	void Info(string msg);
	void PrintError(Exception exception);
	SampleAttribute PrintMainMenu();
	FileShareInfo PrintFileShareInfosMenu(IEnumerable<FileShareInfo> fileShareInfos);
	void PrintRegisteringTransferJob(Guid jobId, PathBase source = null, PathBase destination = null, Guid? existingJobId = null);
}