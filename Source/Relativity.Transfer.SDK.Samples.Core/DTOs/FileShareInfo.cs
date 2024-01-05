﻿using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Relativity.Transfer.SDK.Samples.Core.DTOs;

internal sealed class FileShareInfo(int artifactId, string name, string uncPath) : IEquatable<FileShareInfo>
{
	internal static readonly FileShareInfo BackToToMainMenu = new(int.MinValue, "Back", string.Empty);

	public int ArtifactId { get; init; } = artifactId;
	public string Name { get; init; } = name;
	public string UncPath { get; init; } = uncPath;

	internal bool IsBackToMainMenuOption => Equals(BackToToMainMenu);

	public bool Equals(FileShareInfo other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;

		return ArtifactId == other.ArtifactId && Name == other.Name && UncPath == other.UncPath;
	}

	public static FileShareInfo FromJson(ExpandoObject expando)
	{
		IDictionary<string, object> fields = expando;

		return new FileShareInfo(int.Parse(fields["ArtifactID"].ToString()), fields["Name"].ToString(),
			fields["UNCPath"].ToString());
	}

	public override string ToString()
	{
		return ArtifactId == int.MinValue
			? Name
			: $"Name: {Name}, UNC Path: {UncPath}, ArtifactId: {ArtifactId}";
	}

	public override bool Equals(object obj)
	{
		return ReferenceEquals(this, obj) || (obj is FileShareInfo other && Equals(other));
	}

	public override int GetHashCode()
	{
		return (ArtifactId, Name, UncPath).GetHashCode();
	}
}