using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Nodes;
using Json.Path.QueryExpressions;

namespace Json.Path;

internal class ItemQueryIndex : IArrayIndexExpression, IObjectIndexExpression
{
	private readonly QueryExpressionNode _expression;

	private ItemQueryIndex(QueryExpressionNode expression)
	{
		_expression = expression;
	}

	IEnumerable<int> IArrayIndexExpression.GetIndices(JsonArray array)
	{
		return array.Select((e, i) => (Evaluate(e), i))
			.Where(x => x.Item1)
			.Select(x => x.i);
	}

	IEnumerable<string> IObjectIndexExpression.GetProperties(JsonObject obj)
	{
		return obj.Select(p => (Evaluate(p.Value), p.Key))
			.Where(x => x.Item1)
			.Select(x => x.Key);
	}

	private bool Evaluate(JsonNode? item)
	{
		if (_expression.OutputType != QueryExpressionType.Boolean &&
			_expression.OutputType != QueryExpressionType.InstanceDependent)
			return false;

		var result = _expression.Evaluate(item);
		return result is JsonValue value && 
		       value.TryGetValue(out bool b) && b;
	}

	internal static bool TryParse(ReadOnlySpan<char> span, ref int i, [NotNullWhen(true)] out IIndexExpression? index)
	{
		if (span[i] != '?' || span[i + 1] != '(')
		{
			i = -1;
			index = null;
			return false;
		}

		var localIndex = i + 1;
		if (!span.TryParseExpression(ref localIndex, out var expression) ||
			!(expression.OutputType == QueryExpressionType.Boolean ||
			  expression.OutputType == QueryExpressionType.InstanceDependent))
		{
			i = localIndex;
			index = null;
			return false;
		}

		i = localIndex;
		if (i >= span.Length)
		{
			index = null;
			return false;
		}

		index = new ItemQueryIndex(expression);
		return true;
	}

	public override string ToString()
	{
		return $"?({_expression})";
	}
}