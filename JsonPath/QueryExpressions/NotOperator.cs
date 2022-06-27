using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.QueryExpressions;

internal class NotOperator : IQueryExpressionOperator
{
	public int OrderOfOperation => 1;

	public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
	{
		return QueryExpressionType.Boolean;
	}

	public JsonNode? Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonNode? element)
	{
		var lValue = left.Evaluate(element) as JsonValue;
		if (lValue is null) return JsonNull.SignalNode;

		if (!lValue.TryGetValue(out bool lb)) return null;

		return !lb;
	}

	public string ToString(QueryExpressionNode left, QueryExpressionNode right)
	{
		var lString = left.MaybeAddParentheses(OrderOfOperation);

		return $"!{lString}";
	}
}