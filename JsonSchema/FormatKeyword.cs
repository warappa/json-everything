﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `format`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.Format201909Id)]
[Vocabulary(Vocabularies.FormatAnnotation202012Id)]
[Vocabulary(Vocabularies.FormatAssertion202012Id)]
[JsonConverter(typeof(FormatKeywordJsonConverter))]
public class FormatKeyword : IJsonSchemaKeyword, IEquatable<FormatKeyword>
{
	internal const string Name = "format";

	private static readonly Uri[] _formatAssertionIds =
	{
		new(Vocabularies.Format201909Id),
		new(Vocabularies.FormatAssertion202012Id)
	};

	/// <summary>
	/// The format.
	/// </summary>
	public Format Value { get; }

	/// <summary>
	/// Creates a new <see cref="FormatKeyword"/>.
	/// </summary>
	/// <param name="value">The format.</param>
	public FormatKeyword(Format value)
	{
		Value = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);
		context.LocalResult.SetAnnotation(Name, Value.Key);

		if (Value is UnknownFormat && context.Options.OnlyKnownFormats)
		{
			context.LocalResult.Fail(Name, ErrorMessages.UnknownFormat, ("format", Value.Key));
			return;
		}

		var requireValidation = context.Options.RequireFormatValidation;
		if (!requireValidation)
		{
			var vocabRequirements = context.MetaSchemaVocabs;
			if (vocabRequirements != null)
			{
				foreach (var formatAssertionId in _formatAssertionIds)
				{
					if (vocabRequirements.ContainsKey(formatAssertionId))
					{
						// See https://github.com/json-schema-org/json-schema-spec/pull/1027#discussion_r530068335
						// for why we don't take the vocab value.
						// tl;dr - This implementation understands the assertion vocab, so we apply it,
						// even when the meta-schema says we're not required to.
						requireValidation = true;
						break;
					}
				}
			}
		}

		if (requireValidation && !Value.Validate(context.LocalInstance, out var errorMessage))
		{
			if (Value is UnknownFormat)
				context.LocalResult.Fail(Name, errorMessage);
			else if (errorMessage == null)
				context.LocalResult.Fail(Name, ErrorMessages.Format, ("format", Value.Key));
			else
				context.LocalResult.Fail(Name, ErrorMessages.FormatWithDetail, ("format", Value.Key), ("detail", errorMessage));
		}

		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(FormatKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Equals(Value.Key, other.Value.Key);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as FormatKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}

internal class FormatKeywordJsonConverter : JsonConverter<FormatKeyword>
{
	public override FormatKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var str = reader.GetString()!;
		var format = Formats.Get(str);

		return new FormatKeyword(format);
	}
	public override void Write(Utf8JsonWriter writer, FormatKeyword value, JsonSerializerOptions options)
	{
		writer.WriteString(FormatKeyword.Name, value.Value.Key);
	}
}

public static partial class ErrorMessages
{
	private static string? _unknownFormat;

	/// <summary>
	/// Gets or sets the error message for an unknown format.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[format]] - the format key
	/// </remarks>
	public static string UnknownFormat
	{
		get => _unknownFormat ?? Get();
		set => _unknownFormat = value;
	}

	private static string? _format;

	/// <summary>
	/// Gets or sets the error message for the <see cref="FormatKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[format]] - the format key
	/// </remarks>
	public static string Format
	{
		get => _format ?? Get();
		set => _format = value;
	}

	private static string? _formatWithDetail;

	/// <summary>
	/// Gets or sets the error message for the <see cref="FormatKeyword"/> with
	/// additional information from the format validation.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[format]] - the format key
	///   - [[detail]] - the format key
	/// </remarks>
	public static string FormatWithDetail
	{
		get => _formatWithDetail ?? Get();
		set => _formatWithDetail = value;
	}
}