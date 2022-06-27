using System.Text.Json.Nodes;

namespace Json.Path.QueryExpressions;

internal interface IQueryExpressionOperator
{
	int OrderOfOperation { get; }

	QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right);
	JsonNode? Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonNode? element);
	string ToString(QueryExpressionNode left, QueryExpressionNode right);
}