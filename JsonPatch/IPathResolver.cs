using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Patch;

public interface IPathResolver
{
	JsonElement? Resolve(JsonElement root);
	string ConvertToString();
}

public interface IPathResolverConverter
{
	bool TryParse(string source, out IPathResolver? resolver);
}

public class PathResolverConverter : JsonConverter<IPathResolver>
{
	public List<IPathResolverConverter> Converters { get; } = new()
	{
		new PointerResolverConverter()
	};

	public override IPathResolver Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var content = reader.GetString();
		foreach (var converter in Converters)
		{
			if (converter.TryParse(content, out var resolver)) return resolver!;
		}

		throw new JsonException("Could not find an appropriate resolver to handle the path.");
	}

	public override void Write(Utf8JsonWriter writer, IPathResolver value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}