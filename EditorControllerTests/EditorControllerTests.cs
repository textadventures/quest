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
    }
}
