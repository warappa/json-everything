using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.QueryExpressions;

internal class SubtractionOperator : IQueryExpressionOperator
{
	public int OrderOfOperation => 3;

	public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
	{
		if (left.OutputType != right.OutputType) return QueryExpressionType.Invalid;
		if (left.OutputType == QueryExpressionType.Number) return QueryExpressionType.Number;
		return QueryExpressionType.Invalid;
	}

	public JsonNode? Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonNode? element)
	{
		var lValue = left.Evaluate(element) as JsonValue;
		if (lValue is null) return null;
		var rValue = right.Evaluate(element) as JsonValue;
		if (rValue is null) return null;

		var lNumber = lValue.GetNumber();
		var rNumber = rValue.GetNumber();
		return lNumber is null || rNumber is null
			? null
			: lNumber - rNumber;
	}

	public string ToString(QueryExpressionNode left, QueryExpressionNode right)
	{
		var lString = left.MaybeAddParentheses(OrderOfOperation);
		var rString = right.MaybeAddParentheses(OrderOfOperation, true);

		return $"{lString}-{rString}";
	}
}