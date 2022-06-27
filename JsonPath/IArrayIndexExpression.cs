using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Path;

internal interface IArrayIndexExpression : IIndexExpression
{
	IEnumerable<int> GetIndices(JsonArray array);
}