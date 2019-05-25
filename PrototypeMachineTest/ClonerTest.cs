using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrototypeMachine;
using System;
using System.Collections.Generic;

namespace PrototypeMachineTest
{
    [TestClass]
    public class ClonerTest
    {
        [TestMethod]
        public void Cloner_DeepClone_AsExpected()
        {
            Cloner cloner = new Cloner();
            ClassToClone classToClone = new ClassToClone();

            ClassToClone clone = cloner.DeepClone(classToClone);
            CollectionAssert.AreEqual(classToClone.Children, clone.Children);
            Assert.AreEqual(classToClone.Body, clone.Body);
        }
    }

    [Serializable]
    internal class ClassToClone
    {
        public string Body { get; } = "test text";
        public Dictionary<int, string> Children { get; } = new Dictionary<int, string>
        {
            { 1, "one" },
            { 2, "two" },
            { 3, "three" }
        };
    }
}
