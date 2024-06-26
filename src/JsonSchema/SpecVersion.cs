﻿using System;

namespace Json.Schema;

/// <summary>
/// Enumerates the supported JSON Schema specification versions.
/// </summary>
[Flags]
public enum SpecVersion
{
	/// <summary>
	/// The specification version to use should be determined by the collection of keywords.
	/// </summary>
	Unspecified,
	/// <summary>
	/// JSON Schema Draft 6.
	/// </summary>
	Draft6 = 1,
	/// <summary>
	/// JSON Schema Draft 7.
	/// </summary>
	Draft7 = 1 << 1,
	/// <summary>
	/// JSON Schema Draft 2019-09.
	/// </summary>
	Draft201909 = 1 << 2,
	/// <summary>
	/// JSON Schema Draft 2020-12.
	/// </summary>
	Draft202012 = 1 << 3,
	/// <summary>
	/// JSON Schema Draft Next.
	/// </summary>
	DraftNext = 1 << 4,
	/// <summary>
	/// All versions.
	/// </summary>
	All = Draft6 | Draft7 | Draft201909 | Draft202012 | DraftNext,
}