﻿namespace Relativity.Transfer.SDK.Sample.Authentication
{
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.Dynamic;
	using System.Net.Http;
	using System.Threading.Tasks;

	// More info: https://platform.relativity.com/RelativityOne/Content/REST_API/REST_API_authentication.htm#_Bearer_token_authentication
	internal class BearerTokenRetriever
	{
		private const string IdentityServiceTokenUri = "/Relativity/Identity/connect/token";

		private readonly Uri _baseUri;
		private readonly HttpClient _httpClient;

		public BearerTokenRetriever(string relativityInstanceUrl)
		{
			_baseUri = new Uri(relativityInstanceUrl);
			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("X-CSRF-Header", "-");
		}

		public async Task<string> RetrieveTokenAsync(string clientId, string clientSecret)
		{
			try
			{
				var url = new Uri(_baseUri, IdentityServiceTokenUri);
				var payload = new Dictionary<string, string>
				{
					{ "client_id", clientId },
					{ "client_secret", clientSecret },
					{ "scope", "SystemUserInfo" },
					{ "grant_type", "client_credentials" }
				};

				using (HttpResponseMessage response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(payload)))
				{
					if (!response.IsSuccessStatusCode)
					{
						var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
						throw new InvalidOperationException($"API call {IdentityServiceTokenUri} failed with status code: '{response.StatusCode}' Details: '{content}'.");
					}
					var contentString = await response.Content.ReadAsStringAsync();
					dynamic data = JsonConvert.DeserializeObject<ExpandoObject>(contentString);
					return data.access_token;
				}
			}
			catch (Exception e)
			{
				throw new ApplicationException("Failed to retrieve bearer token.", e);
			}
		}
	}
}
