using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.QueryExpressions;

internal class QueryExpressionNode
{
	private readonly JsonPath? _path;
	private readonly QueryExpressionNode? _left;
	private QueryExpressionNode? _right;
	private QueryExpressionType? _outputType;
	private JsonNode? _value;

	public IQueryExpressionOperator? Operator { get; }

	public QueryExpressionType OutputType => _outputType ??= GetOutputType();

	public QueryExpressionNode(JsonNode? value)
	{
		_value = value ?? JsonNull.SignalNode;
	}

	public QueryExpressionNode(JsonPath path)
	{
		_path = path;
		_outputType = QueryExpressionType.InstanceDependent;
	}

	public QueryExpressionNode(QueryExpressionNode left, IQueryExpressionOperator op, QueryExpressionNode right)
	{
		_left = left ?? throw new ArgumentNullException(nameof(left));
		Operator = op ?? throw new ArgumentNullException(nameof(op));
		_right = right;
	}

	public JsonNode? Evaluate(JsonNode? element)
	{
		if (_value != null) return _value == JsonNull.SignalNode ? null : _value;

		if (_path != null)
		{
			var result = _path.Evaluate(element);
			// don't set _value; need to always eval
			return result.Matches?.Count == 1
				? result.Matches[0].Value
				: null;
		}

		if (OutputType == QueryExpressionType.Invalid) return null;

		var value = Operator!.Evaluate(_left!, _right!, element);
		if (OutputType != QueryExpressionType.InstanceDependent) 
			_value = value;
		return value;
	}

	public void InsertRight(IQueryExpressionOperator op, QueryExpressionNode newRight)
	{
		_right = new QueryExpressionNode(_right!, op, newRight);
	}

	public static bool TryParseSingleValue(ReadOnlySpan<char> span, ref int i, [NotNullWhen(true)] out QueryExpressionNode? node)
	{
		if (span[i] == '!')
		{
			i++;
			if (!TryParseSingleValue(span, ref i, out var singleValue))
			{
				node = null;
				return false;
			}
			node = new QueryExpressionNode(singleValue, Operators.Not, null!);
			return true;
		}

		if (JsonPath.TryParse(span, ref i, true, out var path))
		{
			node = new QueryExpressionNode(path);
			return true;
		}

		if (span.TryParseJsonNode(ref i, out var element))
		{
			node = new QueryExpressionNode(element);
			return true;
		}

		// TODO: this should really be extracting a JsonElement,
		// but I don't know how to parse that from a string with trailing content
		if (span.TryGetInt(ref i, out var value))
		{
			node = new QueryExpressionNode(value);
			return true;
		}

		node = null;
		return false;
	}

	private QueryExpressionType GetOutputType()
	{
		if (_value != null) return GetValueType(_value);
		if (_path != null) return QueryExpressionType.InstanceDependent;

		if (_left?.OutputType == QueryExpressionType.Invalid ||
			_right?.OutputType == QueryExpressionType.Invalid)
			return QueryExpressionType.Invalid;

		// TODO: this might be optimizable depending on the operation
		if (_left?.OutputType == QueryExpressionType.InstanceDependent ||
			_right?.OutputType == QueryExpressionType.InstanceDependent)
			return QueryExpressionType.InstanceDependent;

		return Operator?.GetOutputType(_left!, _right!) ?? GetValueType(_value);
	}

	private static QueryExpressionType GetValueType(JsonNode? node)
	{
		if (node is null) return QueryExpressionType.Null;
		if (node is JsonArray) return QueryExpressionType.Array;
		if (node is JsonObject) return QueryExpressionType.Object;
		if (node is JsonValue value)
		{
			var obj = value.GetValue<object>();
			if (obj is JsonNull) return QueryExpressionType.Null;
			if (obj is JsonElement element) return GetElementValueType(element);
			var objType = obj.GetType();
			if (objType.IsNumber()) return QueryExpressionType.Number;
			if (obj is string) return QueryExpressionType.String;
			if (obj is bool) return QueryExpressionType.Boolean;
		}

		throw new ArgumentOutOfRangeException(nameof(node));
	}

	private static QueryExpressionType GetElementValueType(JsonElement element) =>
		element.ValueKind switch
		{
			JsonValueKind.Object => QueryExpressionType.Object,
			JsonValueKind.Array => QueryExpressionType.Array,
			JsonValueKind.String => QueryExpressionType.String,
			JsonValueKind.Number => QueryExpressionType.Number,
			JsonValueKind.True => QueryExpressionType.Boolean,
			JsonValueKind.False => QueryExpressionType.Boolean,
			JsonValueKind.Null => QueryExpressionType.Null,
			_ => throw new ArgumentOutOfRangeException(nameof(element.ValueKind), element.ValueKind, null)
		};

	public override string ToString()
	{
		return Operator?.ToString(_left!, _right!) ?? _value?.AsJsonString() ?? _path!.ToString();
	}
}