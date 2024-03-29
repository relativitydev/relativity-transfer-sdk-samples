﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Relativity.Transfer.SDK.Samples.Core.Authentication.Credentials;
using Relativity.Transfer.SDK.Samples.Core.Configuration;
using Relativity.Transfer.SDK.Samples.Core.Helpers;
using Relativity.Transfer.SDK.Interfaces.Authentication;

namespace Relativity.Transfer.SDK.Samples.Core.Authentication;

internal class RelativityAuthenticationProvider : IRelativityAuthenticationProvider
{
	private readonly AsyncLock _asyncLock;
	private readonly IBearerTokenRetriever _bearerTokenRetriever;
	private readonly IConsoleLogger _consoleLogger;
	private readonly OAuthCredentials _secretCredentials;
	private string _bearerToken;

	public RelativityAuthenticationProvider(CommonConfiguration common, IConsoleLogger consoleLogger,
		IBearerTokenRetriever bearerTokenRetriever)
	{
		if (HasMissingOAuthParameters(common)) throw new ArgumentException("Invalid Authentication arguments");

		BaseAddress = new Uri(common.InstanceUrl);
		_secretCredentials = common.OAuthCredentials;
		_consoleLogger = consoleLogger;
		_bearerTokenRetriever = bearerTokenRetriever;
		_asyncLock = new AsyncLock();
	}

	public Uri BaseAddress { get; }

	public async Task<RelativityCredentials> GetCredentialsAsync(CancellationToken token)
	{
		try
		{
			using var _ = await _asyncLock.LockAsync(token).ConfigureAwait(false);

			if (!string.IsNullOrWhiteSpace(_bearerToken))
			{
				return new RelativityCredentials(_bearerToken, BaseAddress);
			}

			_consoleLogger.Info("Retrieving bearer token...");
			_bearerToken = await _bearerTokenRetriever.RetrieveTokenAsync(BaseAddress, _secretCredentials)
				.ConfigureAwait(false);

			return new RelativityCredentials(_bearerToken, BaseAddress);
		}
		catch (Exception ex)
		{
			throw new ApplicationException("Failed to retrieve credentials.", ex);
		}
	}

	private static bool HasMissingOAuthParameters(CommonConfiguration common)
	{
		return string.IsNullOrWhiteSpace(common?.InstanceUrl) ||
		       string.IsNullOrWhiteSpace(common.OAuthCredentials?.ClientId) ||
		       string.IsNullOrWhiteSpace(common.OAuthCredentials?.ClientSecret);
	}
}