using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.QueryExpressions;

internal class GreaterThanOrEqualToOperator : IQueryExpressionOperator
{
	public int OrderOfOperation => 4;

	public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
	{
		if (left.OutputType != right.OutputType) return QueryExpressionType.Invalid;
		return left.OutputType switch
		{
			QueryExpressionType.Number => QueryExpressionType.Boolean,
			QueryExpressionType.String => QueryExpressionType.Boolean,
			_ => QueryExpressionType.Invalid
		};
	}

	public JsonNode? Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonNode? element)
	{
		var lValue = left.Evaluate(element) as JsonValue;
		if (lValue is null) return null;
		var rValue = right.Evaluate(element) as JsonValue;
		if (rValue is null) return null;

		if (lValue.IsNumber())
		{
			var lNumber = lValue.GetNumber();
			var rNumber = rValue.GetNumber();
			return lNumber is null || rNumber is null
				? null
				: lNumber >= rNumber;
		}

		return !lValue.TryGetValue(out string? lString) || !rValue.TryGetValue(out string? rString)
			? null
			: string.CompareOrdinal(lString, rString) >= 0;
	}

	public string ToString(QueryExpressionNode left, QueryExpressionNode right)
	{
		var lString = left.MaybeAddParentheses(OrderOfOperation);
		var rString = right.MaybeAddParentheses(OrderOfOperation);

		return $"{lString}>={rString}";
	}
}