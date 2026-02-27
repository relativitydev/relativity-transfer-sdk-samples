# Relativity TransferSDK Samples

## Introduction

This project showcases a straightforward integration scenario with `Relativity.Transfer.SDK` (a.k.a `TransferSDK`) NuGet package.

#### `TransferSDK` lets you:
- Upload a single directory, file or list of items to the selected destination in the RelativityOne fileshare
- Download a single directory or file from the selected source in the RelativityOne fileshare
- Upload a directory (using an existing job based destination path) to the RelativityOne fileshare
- Upload list of items (using an existing job based destination path) to the RelativityOne fileshare
- Download a directory (using an existing job based source path) from the RelativityOne fileshare
- Track the overall progress of the transfer
- Track the progress of individual items that have been transferred, failed, or skipped
- Get a detailed transfer report
- Setup one of three predefined retry policies - No Retry, Linear, Exponential 
- Assign an exclusion policy to filter out unwanted files

The library supports `.NETStandard 2.0`, which means it is cross-platform. You can run it on Windows, Mac or Linux!

#### A sample list of items source file used in above examples:
```
<source path>;<relative destination path>
<source path>;<relative destination path>
...
<source path>;<relative destination path>
```

## Staging Governance compliance <a name = "staging_governance"></a>

Within each tenant or client domain storage area, the Staging Area consists of the four folders listed below. You are only able to write to and access data within these designated Staging Area folders:
- ARM
- StructuredData
- ProcessingSource
- TenantVM

More details can be found in the Staging Governance documentation [here](https://help.relativity.com/RelativityOne/Content/Relativity/Staging_Area.htm).

## Additional information

To ensure optimal security and functionality, it is strongly recommended to avoid using the System Administrator user for transfer operations. Instead, create a dedicated user and assign all necessary permissions to this new user. This approach helps maintain a secure and organized environment, minimizing potential risks associated with using high-privilege users for routine tasks.

## Samples
#### Repository structure:
The repository contains 3 projects:
- `Relativity.Transfer.SDK.Samples.Core` - contains all the interfaces, models, UI, and helpers used by the `Relativity.Transfer.SDK.Samples.Repository` project.
- `Relativity.Transfer.SDK.Samples.Repository` - contains all the samples.
- `Relativity.Transfer.SDK.Samples.CLI` - contains a command-line interface for the `Relativity.Transfer.SDK.Samples.Repository` project. It uses IoC container to create instances of required classes.

#### Examples structure:
- Code presenting a particular example is contained in a single file (see the table below to match a sample with a code file). If additional dependencies are required they are [injected by IoC](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.CLI/Program.cs).
- Usually, all necessary inputs are taken at the beginning of the example using `IConfigurationScreen` implementation.
- Implementation of `IConfigurationScreen` interface prompts a user for every input, but when value is not provided the default value is used (the default value is shown in bracket alongside with prompt, it can be set via `appsettings.json` file).
- Sample code contains accurate comments describing the flow.
- Two types of workflows are supported by samples:
    - **FullPathWorkflow** - consists of samples which requires a source and destination paths to be provided.
    - **JobBasedWorkflow** - consists of samples which requires a source path (upload) or destination path (download) to be provided. The other path is taken from an existing job.

| Name                                       | Config section                      | .Net                                                                                                                                                                                                                                      |
|--------------------------------------------|-------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ___FullPathWorkflow___                     |                                     |                                                                                                                                                                                                                                           |
| BearerTokenAuthentication                  | UploadFile                          | [BearerTokenAuthentication](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/FullPathWorkflow/BearerTokenAuthentication.cs)                                   |
| DownloadDirectory                          | DownloadDirectory                   | [DownloadDirectory](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/FullPathWorkflow/DownloadDirectory.cs)                                                   |
| DownloadFile                               | DownloadFile                        | [DownloadFile](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/FullPathWorkflow/DownloadFile.cs)                                                             |
| SettingUpProgressHandlerAndPrintingSummary | UploadDirectory                     | [SettingUpProgressHandlerAndPrintingSummary](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/FullPathWorkflow/SettingUpProgressHandlerAndPrintingSummary.cs) |
| UploadDirectory                            | UploadDirectory                     | [UploadDirectory](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/FullPathWorkflow/UploadDirectory.cs)                                                       |
| UploadDirectoryWithCustomizedRetryPolicy   | UploadDirectory                     | [UploadDirectoryWithCustomizedRetryPolicy](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/FullPathWorkflow/UploadDirectoryWithCustomizedRetryPolicy.cs)     |
| UploadDirectoryWithExclusionPolicy         | UploadDirectory                     | [UploadDirectoryWithExclusionPolicy](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/FullPathWorkflow/UploadDirectoryWithExclusionPolicy.cs)                 |
| CloudUpload                                | CloudUpload                         | [CloudUpload](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/FullPathWorkflow/CloudUpload.cs)                                                               |
| UploadFile                                 | UploadFile                          | [UploadFile](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/FullPathWorkflow/UploadFile.cs)                                                                 |
| UploadItems                                | UploadFile                          | [UploadItems](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/FullPathWorkflow/UploadItems.cs)                                                               |
| UploadToFilesharePathBasedOnWorkspaceId    | UploadDirectoryByWorkspaceId        | [UploadToFilesharePathBasedOnWorkspaceId](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/FullPathWorkflow/UploadToFileSharePathBasedOnWorkspaceId.cs)       |
| ___JobBasedWorkflow___                     |                                     |                                                                                                                                                                                                                                           |
| DownloadDirectory                          | DownloadDirectory                   | [DownloadDirectory](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/JobBasedWorkflow/DownloadDirectory.cs)                                                   |
| DownloadDirectoryBasedOnExistingJob        | DownloadDirectoryBasedOnExistingJob | [DownloadDirectoryBasedOnExistingJob](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/JobBasedWorkflow/DownloadDirectoryBasedOnExistingJob.cs)               |
| UploadDirectory                            | UploadDirectory                     | [UploadDirectory](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/JobBasedWorkflow/UploadDirectory.cs)                                                       |
| UploadDirectoryBasedOnExistingJob          | UploadDirectoryBasedOnExistingJob   | [UploadDirectoryBasedOnExistingJob](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/JobBasedWorkflow/UploadDirectoryBasedOnExistingJob.cs)                   |
| UploadItems                                | UploadFile                          | [UploadItems](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Repository/JobBasedWorkflow/UploadItems.cs)                                                               |

## Running the sample

- Sample is a regular Visual Studio solution (it is recommended to use Visual Studio >= 2019)
- The settings also can be modified at runtime. They are stored (except the password) in the application's `appsettings.json` file, in the `bin` folder. 
- Whenever the application is rebuilt, the bin `appsettings.json` file is restored to the values from `appsettings.json` file from the repository.
- It is advised to manually remove the `obj` folder and rebuild the solution in order to apply `appsettings.json` file changes into the `bin` config file.
- Directly modifying the `appsettings.json` file in Visual Studio keeps these settings between rebuilds.
- The settings to fill up at application start-up:
    - `Common.ClientName` - This is required to identify the owner of a job. Can be any string.
    - `Common.InstanceUrl`- The URL to the Relativity instance. Example: `https://contoso.relativity.one`
    - `Common.FileShareRoot` - The root of the Relativity fileshare. Example: `\\files.contoso.pod.r1.kcura.com\contoso`
        - Note: This value can be taken from Relativity. Find the `Servers` tab, filter it by `Fileshare`, search for the specified one, and copy its `UNC` path value **without** the `\Files\` suffix.
        - **Note**: It is crucial to remember that backslash `\` characters should be escaped using a double backslash `\\` (this is required by json file to properly deserialize a path).
    - `Common.FileShareRelativePath` - The location where the files are transferred relative to the root of the fileshare.
        - **Note**: `<transfer job id>` folder is created in provided location.
        - **Note**: Provided path can not have `\` at the beginning.
        - The path must be rooted in one of the core folders that reside on the fileshare (like Files, Temp, etc.)
        - **Note**: It is crucial to remember that backslash `\` characters should be escaped using a double backslash `\\` (this is required by json file to properly deserialize a path).
    - `Common.OAuthCredentials` - The OAuth2 secret ID and secret used to authenticate in RelativityOne (Find the `OAuth2 Client` tab, then find the user and copy `ClientId` and `ClientSecret` fileds).
- The settings specified for a sample are commented in `appsettings.json` file. Which section is responsible for which sample is described in the table above.

## Authentication
- First, the transfer client object has to be created, which is used to manage transfers:
```cs
// FullPathWorkflow
var transferClient = TransferClientBuilder.FullPathWorkflow
    .WithAuthentication(authenticationProvider)
    .WithClientName(clientName)
    .WithStagingExplorerContext() // or .WithWorkspaceContext(sampleWorkspaceId) if you base on workspace is
    .Build();

// or JobBasedWorkflow
var transferClient = TransferClientBuilder.JobBasedWorkflow
    .WithAuthentication(authenticationProvider)
    .WithClientName(clientName)
    .WithStagingExplorerContext()
    .Build();
```

- `TransferSDK` uses a bearer token in order to authenticate the transfer.
- To pass the token, the `Relativity.Transfer.SDK.Interfaces.Authentication.IRelativityAuthenticationProvider` must be implemented and an instance should be passed to the client. Sample implementation of authentication provider is [here](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Core/Authentication/RelativityAuthenticationProvider.cs). 
- In order to get the token, [Bearer token authentication](https://platform.relativity.com/RelativityOne/Content/REST_API/REST_API_authentication.htm#_Bearer_token_authentication) is used, which requires user's OAuth2 client id and client's secret. (see [SampleBearerTokenRetriver](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Core/Authentication/BearerTokenRetriever.cs))
    - The token can become obsolete after some time. It's the `IRelativityAuthenticationProvider` responsibility to always return a valid token.
- Client id and secret can be read from the RelativityOne instance. It is under the `Oauth2 Client` tab. Appropriate users can be identified by the `Context User` column.
    - The secret is valid for only a limited period of time (8 hours by default), so it is advised to regenerate it before copying.
- It is strongly recommended to implement your own authentication mechanism which best suits your needs. Here are some helpful links: 
    - [Relativity REST API authentication](https://platform.relativity.com/RelativityOne/Content/REST_API/REST_API_authentication.htm)
    - [OAuth2 clients](https://platform.relativity.com/10.3/Content/Authentication/OAuth2_clients.htm#_OAuth2_Client_Manager_REST_service)
    - [Authentication](https://help.relativity.com/RelativityOne/Content/Relativity/Authentication/Authentication.htm)

## Contributing

- We welcome contributions!
- To add a new sample, create a new class in the appropriate workflow folder in `Relativity.Transfer.SDK.Samples.Repository` project (or create a new folder if a new workflow is used).
- The sample calss should implement [Relativity.Transfer.SDK.Samples.Core.Runner.ISample](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Core/Runner/ISample.cs) interface.
- Add [Relativity.Transfer.SDK.Samples.Core.Attributes.SampleAttribute](https://github.com/relativitydev/relativity-transfer-sdk-samples/blob/main/Source/Relativity.Transfer.SDK.Samples.Core/Attributes/SampleAttribute.cs) to a class which describes the sample. The most important parameter is `TransferType` which specifies which configuration section is used to provide default parameters.
- Whatever you need, should be requested by `.ctor(...)` because the sample is created by IoC container.
- Your sample should be visible in the CLI application.

## Exceptions 
Some exceptions that can be encountered when using samples and their potential root causes

#### System.Net.WebException: The remote name could not be resolved:
- sample console output: 
```
Exception occurred during execution of the transfer. Look at the inner exception for more details.
Relativity.Transfer.SDK.Interfaces.Exceptions.TransferJobExecutionException: Exception occurred during execution of the transfer. Look at the inner exception for more details. ---> Relativity.Transfer.SDK.Interfaces.Exceptions.UnauthorizedException: Unable to retrieve the authentication token. Check inner exception for details. ---> System.ApplicationException: Failed to retrieve credentials. ---> System.ApplicationException: Failed to retrieve bearer token. ---> System.Net.Http.HttpRequestException: An error occurred while sending the request. ---> System.Net.WebException: The remote name could not be resolved: 'reg-b.r1.kcurdda.com'
```
- Reasons: 
    - Wrong RelativityOneInstanceUrl value. 
    - Relativity instance is not available 

#### Relativity.Transfer.SDK.Interfaces.Exceptions.UnauthorizedException:
- sample console output: 
```
Exception occurred during execution of the transfer. Look at the inner exception for more details.
Relativity.Transfer.SDK.Interfaces.Exceptions.TransferJobExecutionException: Exception occurred during execution of the transfer. Look at the inner exception for more details. ---> Relativity.Transfer.SDK.Interfaces.Exceptions.UnauthorizedException: Unable to retrieve the authentication token. Check inner exception for details. ---> System.InvalidOperationException: API call 'ReadAsync' failed with status code: 'Unauthorized' Details: ''.
```
- Reasons: 
    - wrong password/credentials/client ID

#### Relativity.Transfer.SDK.Interfaces.Exceptions.BackendServiceException: Forbidden
- sample console output #1: 
```
Exception occurred during execution of the transfer. Look at the inner exception for more details.
Relativity.Transfer.SDK.Interfaces.Exceptions.TransferJobExecutionException: Exception occurred during execution of the transfer. Look at the inner exception for more details. ---> Relativity.Transfer.SDK.Interfaces.Exceptions.BackendServiceException: Forbidden
```
- Reasons: 
    - Wrong FileshareRelativeDestinationPath setting. Ensure there is no `\` at the beginning of the path.
    - Wrong RelativityOneFileshareRoot. Ensure there is no `files` suffix on the path, and the path is correct.

- sample console output #2:
```
Relativity.Transfer.SDK.Interfaces.Exceptions.BackendServiceException: Staging governance violation. Please use proper staging area such as 'ARM', 'StructuredData', 'ProcessingSource' or 'TenantVM'.
```
- Reasons:
    - Go to [Staging Governance compliance](#staging_governance) paragraph for more information.

#### Relativity.Transfer.SDK.Interfaces.Exceptions.BackendServiceException: Unsupported Media Type:
- sample console output: 
```
Exception occurred during execution of the transfer. Look at the inner exception for more details.
Relativity.Transfer.SDK.Interfaces.Exceptions.TransferJobExecutionException: Exception occurred during execution of the transfer. Look at the inner exception for more details. ---> Relativity.Transfer.SDK.Interfaces.Exceptions.BackendServiceException: Unsupported Media Type
```
- Reasons: 
    - Wrong RelativityOneInstanceUrl. Ensure value is correct, and it ends with `*com` and there is **NO** `/Relativity/` suffix.
