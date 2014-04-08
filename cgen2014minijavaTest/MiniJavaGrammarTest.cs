using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using cgen2014minijava;

namespace cgen2014minijavaTest
{
    [TestClass]
    public class MiniJavaGrammarTest
    {
        [TestMethod]
        public void Constructs()
        {
            MiniJavaGrammar g = new MiniJavaGrammar();
        }
        [TestMethod]
        public void parsesSimple()
        {
            MiniJavaGrammar g = new MiniJavaGrammar();
            String p = "class main { public static void main() { } }";
            g.parse(p);
        }
    }
}
