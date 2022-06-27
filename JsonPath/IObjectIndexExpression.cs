using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Path;

internal interface IObjectIndexExpression : IIndexExpression
{
	IEnumerable<string> GetProperties(JsonObject obj);
}