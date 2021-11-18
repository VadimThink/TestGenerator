using System.Linq;
using System.Threading.Tasks.Dataflow;
using Lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        string code = @"
            namespace TestingNamespace {
                public class FirstClass
                {
                    public void firstMethod(int type) { }
                    public void secondMethod() { }
                }
    
                public class SecondClass
                {
                    private void firstMethod(string password) { }
                    public void secondMethod() { }
                }

                public class ThirdClass
                {
                    private static void firstMethod(string password) { }
                    private void secondMethod(string coded) { }
                }
            }";




        [TestMethod]
        public void TestMethod1()
        {
            var parser = new Parser();
            var classes = parser.getClassDeclarations(parser.parse(code));
            Assert.AreEqual("SecondClass", classes.ElementAt(1).Identifier.ValueText);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var parser = new Parser();
            var generator = new Generator(parser);
            Assert.AreEqual(generator.testCodeFromClassCode(code), generator.testCodeFromClassCode(code));
        }
    }
}