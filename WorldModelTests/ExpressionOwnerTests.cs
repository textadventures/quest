using Shouldly;
using TextAdventures.Quest;
using TextAdventures.Quest.Functions;

namespace WorldModelTests;

[TestClass]
public class ExpressionOwnerTests
{
    [TestMethod]
    public void Test()
    {
        var worldModel = new WorldModel();
        var expressionOwner = new ExpressionOwner(worldModel);

        var dynamicTemplateMethods = expressionOwner.GetFunction("DynamicTemplate");
    }
}