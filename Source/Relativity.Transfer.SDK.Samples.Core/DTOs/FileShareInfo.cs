﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using Relativity.Transfer.SDK.Samples.Core.Helpers;

namespace Relativity.Transfer.SDK.Samples.Core.DTOs;

internal sealed class FileShareInfo : IEquatable<FileShareInfo>
{
	internal static readonly FileShareInfo BackToToMainMenu = new(int.MinValue, "Back", string.Empty);

	private FileShareInfo(int artifactId, string name, string uncPath)
	{
		ArtifactId = artifactId;
		Name = name;
		UncPath = uncPath;
	}

	private int ArtifactId { get; init; }
	public string Name { get; init; }
	public string UncPath { get; init; }

	internal bool IsBackToMainMenuOption => Equals(BackToToMainMenu);

	public bool Equals(FileShareInfo other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;

		return ArtifactId == other.ArtifactId && Name == other.Name && UncPath == other.UncPath;
	}

	public static FileShareInfo FromJson(ExpandoObject expando, IPathExtension pathExtension)
	{
		IDictionary<string, object> fields = expando;

		return new FileShareInfo(int.Parse(fields["ArtifactID"].ToString()), fields["Name"].ToString(),
			pathExtension.GetFileShareRootPath(fields["UNCPath"].ToString()));
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