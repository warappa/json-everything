﻿using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `maxProperties`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.Validation201909Id)]
[Vocabulary(Vocabularies.Validation202012Id)]
[JsonConverter(typeof(MaxPropertiesKeywordJsonConverter))]
public class MaxPropertiesKeyword : IJsonSchemaKeyword, IEquatable<MaxPropertiesKeyword>
{
	internal const string Name = "maxProperties";

	/// <summary>
	/// The maximum expected number of properties.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// Creates a new <see cref="MaxPropertiesKeyword"/>.
	/// </summary>
	/// <param name="value">The maximum expected number of properties.</param>
	public MaxPropertiesKeyword(uint value)
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
			context.WrongValueKind(schemaValueType);
			return;
		}

		var number = ((JsonObject)context.LocalInstance!).Count;
		if (Value < number)
			context.LocalResult.Fail(Name, ErrorMessages.MaxProperties, ("received", number), ("limit", Value));
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(MaxPropertiesKeyword? other)
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
		return Equals(obj as MaxPropertiesKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return (int)Value;
	}
}

internal class MaxPropertiesKeywordJsonConverter : JsonConverter<MaxPropertiesKeyword>
{
	public override MaxPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected a number");

		var number = reader.GetDecimal();
		if (number != Math.Floor(number))
			throw new JsonException("Expected an integer");
		if (number < 0)
			throw new JsonException("Expected a positive integer");

		return new MaxPropertiesKeyword((uint)number);
	}
	public override void Write(Utf8JsonWriter writer, MaxPropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumber(MaxPropertiesKeyword.Name, value.Value);
	}
}

public static partial class ErrorMessages
{
	private static string? _maxProperties;

	/// <summary>
	/// Gets or sets the error message for <see cref="MaxPropertiesKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of properties provided in the JSON instance
	///   - [[limit]] - the upper limit specified in the schema
	/// </remarks>
	public static string MaxProperties
	{
		get => _maxProperties ?? Get();
		set => _maxProperties = value;
	}
}