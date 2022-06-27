using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.QueryExpressions;

internal class OrOperator : IQueryExpressionOperator
{
	public int OrderOfOperation => 5;

	public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
	{
		if (left.OutputType != right.OutputType) return QueryExpressionType.Invalid;
		if (left.OutputType == QueryExpressionType.Boolean) return QueryExpressionType.Boolean;
		return QueryExpressionType.Invalid;
	}

	public JsonNode? Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonNode? element)
	{
		var lValue = left.Evaluate(element) as JsonValue;
		if (lValue is null) return JsonNull.SignalNode;
		var rValue = right.Evaluate(element) as JsonValue;
		if (rValue is null) return JsonNull.SignalNode;

		if (!lValue.TryGetValue(out bool lb) || !rValue.TryGetValue(out bool rb))
			return null;

		return lb || rb;
	}

	public string ToString(QueryExpressionNode left, QueryExpressionNode right)
	{
		var lString = left.MaybeAddParentheses(OrderOfOperation);
		var rString = right.MaybeAddParentheses(OrderOfOperation);

		return $"{lString}||{rString}";
	}
}