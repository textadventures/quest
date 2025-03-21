﻿using QuestViva.Engine;

namespace QuestViva.EngineTests
{
    [TestClass]
    public class FieldExtensionTests
    {
        [TestMethod]
        public void TestListExtension_Simple()
        {
            WorldModel worldModel = Helpers.CreateWorldModel();

            Element type1 = worldModel.GetElementFactory(ElementType.ObjectType).Create("type1");
            type1.Fields.AddFieldExtension("listfield", new QuestList<string>(new[] { "a" }, true));

            Element type2 = worldModel.GetElementFactory(ElementType.ObjectType).Create("type2");
            type2.Fields.AddFieldExtension("listfield", new QuestList<string>(new[] { "b" }, true));

            Element obj = worldModel.GetElementFactory(ElementType.Object).Create("object");
            obj.Fields.AddType(type1);
            obj.Fields.AddType(type2);

            var result = obj.Fields.GetAsType<QuestList<string>>("listfield");

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("a"));
            Assert.IsTrue(result.Contains("b"));
        }

        [TestMethod]
        public void TestListExtension_InheritingDirectly()
        {
            WorldModel worldModel = Helpers.CreateWorldModel();

            Element type1 = worldModel.GetElementFactory(ElementType.ObjectType).Create("type1");
            type1.Fields.AddFieldExtension("listfield", new QuestList<string>(new[] { "a" }, true));

            Element type2 = worldModel.GetElementFactory(ElementType.ObjectType).Create("type2");
            type2.Fields.AddType(type1);
            type2.Fields.AddFieldExtension("listfield", new QuestList<string>(new[] { "b" }, true));

            Element obj = worldModel.GetElementFactory(ElementType.Object).Create("object");
            obj.Fields.AddType(type2);

            var result = obj.Fields.GetAsType<QuestList<string>>("listfield");

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("a"));
            Assert.IsTrue(result.Contains("b"));
        }

        [TestMethod]
        public void TestListExtension_InheritingIndirectly()
        {
            WorldModel worldModel = Helpers.CreateWorldModel();

            Element type1 = worldModel.GetElementFactory(ElementType.ObjectType).Create("type1");
            type1.Fields.AddFieldExtension("listfield", new QuestList<string>(new[] { "a" }, true));

            Element type2 = worldModel.GetElementFactory(ElementType.ObjectType).Create("type2");
            type2.Fields.AddType(type1);
            type2.Fields.AddFieldExtension("listfield", new QuestList<string>(new[] { "b" }, true));

            Element type3 = worldModel.GetElementFactory(ElementType.ObjectType).Create("type3");
            type3.Fields.AddType(type2);

            Element obj = worldModel.GetElementFactory(ElementType.Object).Create("object");
            obj.Fields.AddType(type3);

            var result = obj.Fields.GetAsType<QuestList<string>>("listfield");

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("a"));
            Assert.IsTrue(result.Contains("b"));
        }
    }
}
