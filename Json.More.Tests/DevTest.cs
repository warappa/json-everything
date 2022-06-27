using NUnit.Framework;

namespace Json.More.Tests;

public class DevTest
{

	[Test]
	[Ignore("test")]
	public void Test()
	{
		var node = JsonNull.SignalNode;

		Assert.IsNotNull(node);
		Assert.AreEqual("null", node.AsJsonString());
	}
}
