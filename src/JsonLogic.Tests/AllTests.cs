﻿using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;
using TestHelpers;

namespace Json.Logic.Tests;

public class AllTests
{
	[Test]
	public void AllMatchCondition()
	{
		var rule = new AllRule(JsonNode.Parse("[1,2,3]"),
			new MoreThanRule(new VariableRule(""), 0));

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void AllDoNotMatchCondition()
	{
		var rule = new AllRule(JsonNode.Parse("[1,-2,3]"),
			new MoreThanRule(new VariableRule(""), 0));

		JsonAssert.IsFalse(rule.Apply());
	}
}