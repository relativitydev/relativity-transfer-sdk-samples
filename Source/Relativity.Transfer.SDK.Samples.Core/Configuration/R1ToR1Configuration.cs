using Relativity.Transfer.SDK.Samples.Core.Authentication.Credentials;

namespace Relativity.Transfer.SDK.Samples.Core.Configuration;

/// <summary>
///     Configuration of the R1-to-R1 (RelativityOne-to-RelativityOne) sample.
///     The source (Sender) instance is described by the <see cref="CommonConfiguration" /> section,
///     the destination (Receiver) instance is described here.
/// </summary>
internal sealed record R1ToR1Configuration(
	string Source,
	string Destination,
	string DestinationInstanceUrl,
	OAuthCredentials DestinationOAuthCredentials)
{
	internal static readonly R1ToR1Configuration Empty =
		new(string.Empty, string.Empty, string.Empty, new OAuthCredentials(string.Empty, string.Empty));
}
