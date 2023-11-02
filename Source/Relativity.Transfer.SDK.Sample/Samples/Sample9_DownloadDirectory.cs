﻿using System;
using System.Threading.Tasks;
using Relativity.Transfer.SDK.Sample.Authentication;
using Relativity.Transfer.SDK.Sample.Authentication.Credentials;
using Relativity.Transfer.SDK.Sample.Helpers;
using Relativity.Transfer.SDK.Sample.Monitoring;

namespace Relativity.Transfer.SDK.Sample.Samples
{
    internal class Sample9_DownloadDirectory : SampleBase
    {
        public Sample9_DownloadDirectory(ConsoleHelper consoleHelper) : base(consoleHelper) { }
        
        public override async Task ExecuteAsync()
        {
            Console.WriteLine("Settings: ");

            var clientName = _consoleHelper.GetOrEnterSetting(SettingNames.ClientName);
            var relativityInstanceAddress = _consoleHelper.GetOrEnterSetting(SettingNames.RelativityOneInstanceUrl);
            var clientId = _consoleHelper.GetOrEnterSetting(SettingNames.ClientOAuth2Id);
            var clientSecret = _consoleHelper.GetOrEnterSetting(SettingNames.ClientSecret);
            var transferJobId = Guid.NewGuid();
            _consoleHelper.SetupTransferJobId(transferJobId);
            var sourcePath = _consoleHelper.EnterSourceDirectoryPathOrTakeDefault();
            var destinationPath = _consoleHelper.GetDestinationDirectoryPath();

            var authenticationProvider = new RelativityAuthenticationProvider(relativityInstanceAddress, new OAuthCredentials(clientId, clientSecret));

            // Builder follows Fluent convention, we'll add more options in the future. The only required component (beside client name)
            // is the authentication provider - we have provided one that utilizes OAuth based approach, but you can create your own.
            var transferClient = TransferClientBuilder.Instance
                .WithAuthentication(authenticationProvider)
                .WithClientName(clientName)
                .Build();

            Console.WriteLine();
            Console.WriteLine($"Creating transfer \"{transferJobId}\" {Environment.NewLine}   - From:  {sourcePath} {Environment.NewLine}   - To:  {destinationPath}");
            Console.WriteLine();

            var result = await transferClient
                .DownloadDirectoryAsync(transferJobId, sourcePath, destinationPath, ConsoleStatisticHook.GetProgressHandler(), default)
                .ConfigureAwait(false);

            Console.WriteLine();
            Console.WriteLine($"Transfer has finished: ");
            Console.WriteLine(new TransferJobSummary(result));
        }


    }
}