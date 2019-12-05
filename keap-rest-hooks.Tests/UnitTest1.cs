using System;
using System.IO;
using keap_rest_hooks.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace keap_rest_hooks.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string[] lines = { "First line", "Second line", "Third line" };

            File.WriteAllLines(@"C:\Projects\WriteLines.txt", lines);
        }
    }
}
