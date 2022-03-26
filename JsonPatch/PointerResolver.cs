using System.Text.Json;
using Json.Pointer;

namespace Json.Patch;

public class PointerResolver : IPathResolver
{
	public static readonly PointerResolver Empty = JsonPointer.Empty;

	public JsonPointer Pointer { get; }

	public PointerResolver(JsonPointer pointer)
	{
		Pointer = pointer;
	}

	public JsonElement? Resolve(JsonElement root)
	{
		return Pointer.Evaluate(root);
	}

	public string ConvertToString()
	{
		return Pointer.ToString();
	}

	public static implicit operator PointerResolver(JsonPointer pointer)
	{
		return new PointerResolver(pointer);
	}
}

public class PointerResolverConverter : IPathResolverConverter
{
	public bool TryParse(string source, out IPathResolver? resolver)
	{
		if (JsonPointer.TryParse(source, out var pointer))
		{
			resolver = new PointerResolver(pointer!);
			return true;
		}

		resolver = null;
		return false;

	}
}