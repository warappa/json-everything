﻿using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `minProperties`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.Validation201909Id)]
[Vocabulary(Vocabularies.Validation202012Id)]
[JsonConverter(typeof(MinPropertiesKeywordJsonConverter))]
public class MinPropertiesKeyword : IJsonSchemaKeyword, IEquatable<MinPropertiesKeyword>
{
	internal const string Name = "minProperties";

	/// <summary>
	/// The minimum expected number of properties.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// Creates a new <see cref="MinPropertiesKeyword"/>.
	/// </summary>
	/// <param name="value">The minimum expected number of properties.</param>
	public MinPropertiesKeyword(uint value)
	{
		Value = value;
	}

	/// <summary>
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);
		var schemaValueType = context.LocalInstance.GetSchemaValueType();
		if (schemaValueType != SchemaValueType.Object)
		{
			context.LocalResult.Pass();
			context.WrongValueKind(schemaValueType);
			return;
		}

		var number = ((JsonObject)context.LocalInstance!).Count;
		if (Value <= number)
			context.LocalResult.Pass();
		else
			context.LocalResult.Fail(Name, ErrorMessages.MinProperties, ("received", number), ("limit", Value));
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(MinPropertiesKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Value == other.Value;
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as MinPropertiesKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return (int)Value;
	}
}

internal class MinPropertiesKeywordJsonConverter : JsonConverter<MinPropertiesKeyword>
{
	public override MinPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected a number");

		var number = reader.GetDecimal();
		if (number != Math.Floor(number))
			throw new JsonException("Expected an integer");
		if (number < 0)
			throw new JsonException("Expected a positive integer");

		return new MinPropertiesKeyword((uint)number);
	}
	public override void Write(Utf8JsonWriter writer, MinPropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumber(MinPropertiesKeyword.Name, value.Value);
	}
}

public static partial class ErrorMessages
{
	private static string? _minProperties;

	/// <summary>
	/// Gets or sets the error message for <see cref="MinPropertiesKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of properties provided in the JSON instance
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string MinProperties
	{
		get => _minProperties ?? Get();
		set => _minProperties = value;
	}
}