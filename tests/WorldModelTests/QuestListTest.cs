using TextAdventures.Quest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace WorldModelTests
{
    [TestClass()]
    public class QuestListTest
    {
        private WorldModel m_worldModel;
        Element a, b, c;

        [TestInitialize]
        public void Setup()
        {
            m_worldModel = new WorldModel();

            a = m_worldModel.GetElementFactory(ElementType.Object).Create("a");
            b = m_worldModel.GetElementFactory(ElementType.Object).Create("b");
            c = m_worldModel.GetElementFactory(ElementType.Object).Create("c");
        }

        [TestMethod()]
        public void ExcludeTest()
        {
            //string lists
            var stringList = new QuestList<string>() { "a", "a", "b", "c" };

            var expected = new QuestList<string>() { "b", "c" };
            var actual = stringList.Exclude("a");
            Assert.IsTrue(actual.SequenceEqual(expected));

            //element lists
            var elList = new QuestList<Element>() { a, a, b, c };

            var expectedEList = new QuestList<Element>() { b, c };
            var actualEList = elList.Exclude(a);
            Assert.IsTrue(actualEList.SequenceEqual(expectedEList));
        }

        [TestMethod()]
        public void ExcludeWithListTest()
        {
            //string lists
            var stringList = new QuestList<string>() { "a", "a", "b", "c" };
            var excludeList = new QuestList<string>() { "a", "b" };

            var expected = new QuestList<string>() { "c" };
            var actual = stringList.Exclude(excludeList);
            Assert.IsTrue(actual.SequenceEqual(expected));

            //element lists
            var elList = new QuestList<Element>() { a, a, b, c };
            var excludeEList = new QuestList<Element>() { a, b };

            var expectedEList = new QuestList<Element>() { c };
            var actualEList = elList.Exclude(excludeEList);
            Assert.IsTrue(actualEList.SequenceEqual(expectedEList));
        }
    }
}
