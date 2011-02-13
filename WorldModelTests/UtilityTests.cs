using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AxeSoftware.Quest;

namespace WorldModelTests
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void TestConvertDottedProperties()
        {
            Assert.AreEqual("obj_prop", Utility.ConvertDottedPropertiesToVariable("obj.prop"));
            Assert.AreEqual("obj1_prop obj2_prop", Utility.ConvertDottedPropertiesToVariable("obj1.prop obj2.prop"));
            Assert.AreEqual("(\"myfile.html\")", Utility.ConvertDottedPropertiesToVariable("(\"myfile.html\")"));
            Assert.AreEqual("\"myfile.html\"", Utility.ConvertDottedPropertiesToVariable("\"myfile.html\""));
            Assert.AreEqual("obj1_prop \"test.html\" obj2_prop", Utility.ConvertDottedPropertiesToVariable("obj1.prop \"test.html\" obj2.prop"));
        }
    }
}
