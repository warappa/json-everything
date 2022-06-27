using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Path;

internal class LengthSelector : SelectorBase
{
	public static LengthSelector Instance { get; } = new();

	private LengthSelector() { }

	protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
	{
		switch (match.Value)
		{
			case JsonObject obj:
				yield return new PathMatch(obj.Count, match.Location.AddSelector(new PropertySelector("length")));
				break;
			case JsonArray array:
				yield return new PathMatch(array.Count, match.Location.AddSelector(new PropertySelector("length")));
				break;
		}
	}

	public override string ToString()
	{
		return ".length";
	}
}