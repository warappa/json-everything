using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Path;

internal class RecursiveDescentSelector : SelectorBase
{
	protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
	{
		return GetChildren(match);
	}

	private static IEnumerable<PathMatch> GetChildren(PathMatch match)
	{
		switch (match.Value)
		{
			case JsonObject obj:
				yield return match;
				foreach (var prop in obj)
				{
					var newMatch = new PathMatch(prop.Value, match.Location.AddSelector(new IndexSelector(new[] { (PropertyNameIndex)prop.Key })));
					foreach (var child in GetChildren(newMatch))
					{
						yield return child;
					}
				}
				break;
			case JsonArray array:
				yield return match;
				foreach (var (item, index) in array.Select((item, i) => (item, i)))
				{
					var newMatch = new PathMatch(item, match.Location.AddSelector(new IndexSelector(new[] { (SimpleIndex)index })));
					foreach (var child in GetChildren(newMatch))
					{
						yield return child;
					}
				}
				break;
			case null:
			case JsonValue:
				yield return match;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public override string ToString()
	{
		return ".";
	}
}