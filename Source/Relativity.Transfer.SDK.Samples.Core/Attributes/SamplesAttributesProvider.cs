using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Relativity.Transfer.SDK.Samples.Core.Attributes;

internal static class SamplesAttributesProvider
{
	// Only the samples assemblies carry [Sample]. Scanning third-party assemblies (including the SDK) would make
	// the CLI fail to start whenever one of them has a type that cannot be loaded.
	private const string SamplesAssemblyPrefix = "Relativity.Transfer.SDK.Samples.";

	public static SampleAttribute[] GetSamplesAttributes()
	{
		EnsureAllSamplesAssembliesAreLoaded();

		return AppDomain.CurrentDomain
			.GetAssemblies()
			.Where(IsSamplesAssembly)
			.SelectMany(assembly => assembly.GetTypes())
			.Select(type => type.GetCustomAttribute<SampleAttribute>(false))
			.Where(attr => attr != null)
			.Concat(new[] { SampleAttribute.ExitOptionAttribute })
			.Distinct()
			.OrderBy(attr => attr.Order)
			.ToArray();
	}

	private static bool IsSamplesAssembly(Assembly assembly) =>
		assembly.GetName().Name?.StartsWith(SamplesAssemblyPrefix, StringComparison.Ordinal) == true;

	private static void EnsureAllSamplesAssembliesAreLoaded()
	{
		var directoryName = Path.GetDirectoryName(typeof(SampleAttribute).Assembly.Location);
		if (string.IsNullOrWhiteSpace(directoryName))
			throw new InvalidOperationException("Cannot get directory path of the Core assembly.");

		foreach (var assemblyPath in Directory.GetFiles(directoryName, SamplesAssemblyPrefix + "*.dll"))
			Assembly.LoadFrom(assemblyPath);
	}
}
