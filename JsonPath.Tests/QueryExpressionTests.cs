using System.Text.Json.Nodes;
using Json.More;
using Json.Path.QueryExpressions;
using NUnit.Framework;

namespace Json.Path.Tests;

public class QueryExpressionTests
{
	[Test]
	public void NumberAddition()
	{
		var exp = new QueryExpressionNode(
			new QueryExpressionNode(4),
			Operators.Addition,
			new QueryExpressionNode(5)
		);

		Assert.AreEqual(QueryExpressionType.Number, exp.OutputType);
		Assert.IsTrue(((JsonNode)9).IsEquivalentTo(exp.Evaluate(default)));
		Assert.AreEqual("4+5", exp.ToString());
	}

	[Test]
	public void StringAddition()
	{
		var exp = new QueryExpressionNode(
			new QueryExpressionNode("yes"),
			Operators.Addition,
			new QueryExpressionNode("maybe")
		);

		Assert.AreEqual(QueryExpressionType.String, exp.OutputType);
		Assert.IsTrue(((JsonNode?)"yesmaybe").IsEquivalentTo(exp.Evaluate(default)));
		Assert.AreEqual("\"yes\"+\"maybe\"", exp.ToString());
	}

	[Test]
	public void MixedAddition()
	{
		var exp = new QueryExpressionNode(
			new QueryExpressionNode("yes"),
			Operators.Addition,
			new QueryExpressionNode(5)
		);

		Assert.AreEqual(QueryExpressionType.Invalid, exp.OutputType);
		Assert.AreEqual("\"yes\"+5", exp.ToString());
	}

	[Test]
	public void DivisionByZero()
	{
		var exp = new QueryExpressionNode(
			new QueryExpressionNode(4),
			Operators.Division,
			new QueryExpressionNode(0)
		);

		Assert.AreEqual(QueryExpressionType.Number, exp.OutputType);
		Assert.AreEqual(null, exp.Evaluate(default));
		Assert.AreEqual("4/0", exp.ToString());
	}

	[Test]
	public void Division()
	{
		var exp = new QueryExpressionNode(
			new QueryExpressionNode(8),
			Operators.Division,
			new QueryExpressionNode(5)
		);

		Assert.AreEqual(QueryExpressionType.Number, exp.OutputType);
		Assert.IsTrue(((JsonNode)1.6).IsEquivalentTo(exp.Evaluate(default)));
		Assert.AreEqual("8/5", exp.ToString());
	}

	[Test]
	public void LessThan_False()
	{
		var exp = new QueryExpressionNode(
			new QueryExpressionNode(8),
			Operators.LessThan,
			new QueryExpressionNode(5)
		);

		Assert.AreEqual(QueryExpressionType.Boolean, exp.OutputType);
		Assert.IsTrue(((JsonNode)false).IsEquivalentTo(exp.Evaluate(default)));
		Assert.AreEqual("8<5", exp.ToString());
	}

	[Test]
	public void LessThan_True()
	{
		var exp = new QueryExpressionNode(
			new QueryExpressionNode(4),
			Operators.LessThan,
			new QueryExpressionNode(5)
		);

		Assert.AreEqual(QueryExpressionType.Boolean, exp.OutputType);
		Assert.IsTrue(((JsonNode)true).IsEquivalentTo(exp.Evaluate(default)));
		Assert.AreEqual("4<5", exp.ToString());
	}

	[Test]
	public void And_True_True()
	{
		var exp = new QueryExpressionNode(
			new QueryExpressionNode(true),
			Operators.And,
			new QueryExpressionNode(true)
		);

		Assert.AreEqual(QueryExpressionType.Boolean, exp.OutputType);
		Assert.IsTrue(((JsonNode)true).IsEquivalentTo(exp.Evaluate(default)));
		Assert.AreEqual("true&&true", exp.ToString());
	}

	[Test]
	public void And_False_True()
	{
		var exp = new QueryExpressionNode(
			new QueryExpressionNode(false),
			Operators.And,
			new QueryExpressionNode(true)
		);

		Assert.AreEqual(QueryExpressionType.Boolean, exp.OutputType);
		Assert.IsTrue(((JsonNode)false).IsEquivalentTo(exp.Evaluate(default)));
		Assert.AreEqual("false&&true", exp.ToString());
	}

	[Test]
	public void And_True_False()
	{
		var exp = new QueryExpressionNode(
			new QueryExpressionNode(true),
			Operators.And,
			new QueryExpressionNode(false)
		);

		Assert.AreEqual(QueryExpressionType.Boolean, exp.OutputType);
		Assert.IsTrue(((JsonNode)false).IsEquivalentTo(exp.Evaluate(default)));
		Assert.AreEqual("true&&false", exp.ToString());
	}

	[Test]
	public void And_False_False()
	{
		var exp = new QueryExpressionNode(
			new QueryExpressionNode(false),
			Operators.And,
			new QueryExpressionNode(false)
		);

		Assert.AreEqual(QueryExpressionType.Boolean, exp.OutputType);
		Assert.IsTrue(((JsonNode)false).IsEquivalentTo(exp.Evaluate(default)));
		Assert.AreEqual("false&&false", exp.ToString());
	}
}