using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;

namespace WorldModelTests
{
    [TestClass]
    public class ComplexTypesTests
    {
        private WorldModel m_worldModel;
        private Element m_object;

        [TestInitialize]
        public void Setup()
        {
            m_worldModel = new WorldModel();

            m_object = m_worldModel.GetElementFactory(ElementType.Object).Create("object");
            var list = new QuestList<object> {"string1"};
            var dictionary = new QuestDictionary<object> {{"key1", "nested string"}};
            list.Add(dictionary);
            m_object.Fields.Set("list", list);
            m_object.Fields.Resolve(null);
        }

        [TestMethod]
        public void TestSetup()
        {
            var obj = m_worldModel.Elements.Get("object");
            Assert.IsNotNull(obj);
            var list = obj.Fields.GetAsType<QuestList<object>>("list");
            Assert.IsNotNull(list);
            Assert.AreEqual("string1", list[0]);
            var dictionary = list[1] as QuestDictionary<object>;
            Assert.IsNotNull(dictionary);
            Assert.IsTrue(dictionary.ContainsKey("key1"));
            Assert.AreEqual("nested string", dictionary["key1"]);
        }

        [TestMethod]
        public void TestAddListItemUndoRedo()
        {
            var obj = m_worldModel.Elements.Get("object");
            var list = obj.Fields.GetAsType<QuestList<object>>("list");
            Assert.AreEqual(2, list.Count);

            m_worldModel.UndoLogger.StartTransaction("Add list item");
            list.Add("new item");
            m_worldModel.UndoLogger.EndTransaction();

            Assert.AreEqual(3, list.Count);

            m_worldModel.UndoLogger.Undo();
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void TestNestedDictionaryAddUndoRedo()
        {
            var obj = m_worldModel.Elements.Get("object");
            var list = obj.Fields.GetAsType<QuestList<object>>("list");
            var dictionary = list[1] as QuestDictionary<object>;
            Assert.AreEqual(1, dictionary.Count);

            m_worldModel.UndoLogger.StartTransaction("Add dictionary item");
            dictionary.Add("key2", "new string");
            m_worldModel.UndoLogger.EndTransaction();

            Assert.AreEqual(2, dictionary.Count);

            m_worldModel.UndoLogger.Undo();
            Assert.AreEqual(1, dictionary.Count);
        }
    }
}
