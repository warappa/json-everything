using System.Text.Json.Nodes;

namespace Json.Path.QueryExpressions;

internal class ExistsOperator : IQueryExpressionOperator
{
	public int OrderOfOperation => 1;

	public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
	{
		return QueryExpressionType.Boolean;
	}

	public JsonNode? Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonNode? element)
	{
		return left.Evaluate(element) != null;
	}

	public string ToString(QueryExpressionNode left, QueryExpressionNode right)
	{
		return left.ToString();
	}
}