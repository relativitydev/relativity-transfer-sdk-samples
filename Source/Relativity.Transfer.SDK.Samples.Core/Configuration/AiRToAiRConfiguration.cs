using Relativity.Transfer.SDK.Samples.Core.Authentication.Credentials;

namespace Relativity.Transfer.SDK.Samples.Core.Configuration;

/// <summary>
///     Configuration of the aiR-to-aiR (Relativity aiR-to-Relativity aiR) sample.
///     The source (Sender) instance is described by the <see cref="CommonConfiguration" /> section,
///     the destination (Receiver) instance is described here.
/// </summary>
internal sealed record AiRToAiRConfiguration(
	string Source,
	string Destination,
	string DestinationInstanceUrl,
	OAuthCredentials DestinationOAuthCredentials)
{
	internal static readonly AiRToAiRConfiguration Empty =
		new(string.Empty, string.Empty, string.Empty, new OAuthCredentials(string.Empty, string.Empty));
}
