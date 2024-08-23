using TextAdventures.Quest;

namespace EditorCoreTests
{
    [TestClass]
    public class EditorControllerTests : EditorControllerTestBase
    {
        [TestMethod]
        public void TestValidElementNames()
        {
            AssertValid("object");
            AssertValid("objéct");
            AssertValid("objɘct");
            AssertValid("object name");
            AssertValid("object_name");
        }

        [TestMethod]
        public void TestInvalidElementNames()
        {
            AssertInvalid("object>");
            AssertInvalid("object\"");
            AssertInvalid("object and object");
        }

        private void AssertValid(string name)
        {
            var result = Controller.CanAdd(name);
            Assert.IsTrue(result.Valid, string.Format("Error adding {0}: {1}", name, result.Message.ToString()));
        }

        private void AssertInvalid(string name)
        {
            var result = Controller.CanAdd(name);
            Assert.IsFalse(result.Valid, string.Format("Expected adding '{0}' to fail", name));
        }

        [TestMethod]
        public void TestValidExpressionsWithBrackets()
        {
            AssertValidExpression("");
            AssertValidExpression("some expression");
            AssertValidExpression("(some expression)");
            AssertValidExpression("some (valid (expression))");
            AssertValidExpression("this is valid because \"the bracket ( is in quotes\"");
        }

        [TestMethod]
        public void TestInvalidExpressionsWithBrackets()
        {
            AssertInvalidExpression("invalid (expression");
            AssertInvalidExpression("(invalid) expression(");
        }

        [TestMethod]
        public void TestValidExpressionsWithQuotes()
        {
            AssertValidExpression("\"this is in quotes\"");
            AssertValidExpression("\"this is in quotes\" + \"also this is in quotes\"");
        }

        [TestMethod]
        public void TestInvalidExpressionsWithQuotes()
        {
            AssertInvalidExpression("\"no end quote");
            AssertInvalidExpression("\"this is in quotes\" + \"but this doesn't have an end quote");
        }

        [TestMethod]
        public void TestCanAddVerb()
        {
            EditorController.CanAddVerbResult cavr = Controller.CanAddVerb("kick");
            Assert.IsTrue(cavr.CanAdd);
        }

        [TestMethod]
        public void TestCanAddVerb_Rejected()
        {
            EditorController.CanAddVerbResult cavr = Controller.CanAddVerb("enter");
            Assert.AreEqual("enter", cavr.ClashingCommand);
            cavr = Controller.CanAddVerb("kick;look");
            Assert.AreEqual("look", cavr.ClashingCommand);
            cavr = Controller.CanAddVerb("kick ; ask ; sing");
            Assert.AreEqual("ask", cavr.ClashingCommand);
        }

        private void AssertValidExpression(string expression)
        {
            var result = Controller.ValidateExpression(expression);
            Assert.IsTrue(result.Valid, string.Format("Expected expression '{0}' to be evaluated as valid: {1}", expression, result.Message.ToString()));
        }

        private void AssertInvalidExpression(string expression)
        {
            var result = Controller.ValidateExpression(expression);
            Assert.IsFalse(result.Valid, string.Format("Expected expression '{0}' to be evaluated as invalid", expression));
        }
    }
}
