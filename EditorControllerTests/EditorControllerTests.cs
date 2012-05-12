using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AxeSoftware.Quest;

namespace EditorControllerTests
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
        public void TestInalidElementNames()
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
        public void TestValidExpressions()
        {
            AssertValidExpression("");
            AssertValidExpression("some expression");
            AssertValidExpression("(some expression)");
            AssertValidExpression("some (valid (expression))");
        }

        [TestMethod]
        public void TestInvalidExpressions()
        {
            AssertInvalidExpression("invalid (expression");
            AssertInvalidExpression("(invald) expression(");
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
