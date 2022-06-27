using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path;

internal class IndexSelector : SelectorBase
{
	private readonly List<IIndexExpression>? _ranges;

	public IndexSelector(IEnumerable<IIndexExpression>? ranges)
	{
		_ranges = ranges?.ToList();
	}

	protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
	{
		switch (match.Value)
		{
			case JsonArray array:
				var indices = _ranges?.OfType<IArrayIndexExpression>()
					              .SelectMany(r => r.GetIndices(array))
					              .Where(i => 0 <= i && i < array.Count)
					              .Distinct() ??
				              Enumerable.Range(0, array.Count);
				foreach (var index in indices)
				{
					yield return new PathMatch(array[index], match.Location.AddSelector(new IndexSelector(new[] { (SimpleIndex)index })));
				}
				break;
			case JsonObject obj:
				if (_ranges != null)
				{
					var props = _ranges.OfType<IObjectIndexExpression>()
						.SelectMany(r => r.GetProperties(obj))
						.Distinct();
					foreach (var prop in props)
					{
						if (!obj.TryGetValue(prop, out var value, out _)) continue;
						yield return new PathMatch(value, match.Location.AddSelector(new IndexSelector(new[] { (PropertyNameIndex)prop })));
					}
				}
				else
				{
					foreach (var prop in obj)
					{
						yield return new PathMatch(prop.Value, match.Location.AddSelector(new IndexSelector(new[] { (PropertyNameIndex)prop.Key })));
					}
				}
				break;
		}
	}

	public override string ToString()
	{
		return _ranges == null ? "[*]" : $"[{string.Join(",", _ranges.Select(r => r.ToString()))}]";
	}
}