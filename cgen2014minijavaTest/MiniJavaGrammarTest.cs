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
        [TestMethod]
        public void parsesNormalIf()
        {
            MiniJavaGrammar g = new MiniJavaGrammar();
            String p = "class main { public static void main() { if (1) assert(1); else assert(1); } }";
            SyntaxTree t = g.parse(p);
            Assert.AreEqual(new NonTerminal("if statement"), t.root.children[0].children[7].children[0].children[0].token);
            Assert.AreEqual(6, t.root.children[0].children[7].children[0].children[0].children.Count);
        }
        [TestMethod]
        public void parsesDanglingIf()
        {
            MiniJavaGrammar g = new MiniJavaGrammar();
            String p = "class main { public static void main() { if (1) if (1) assert(1); else assert(1); } }";
            SyntaxTree t = g.parse(p);
            Assert.AreEqual(5, t.root.children[0].children[7].children[0].children[0].children.Count);
            Assert.AreEqual(6, t.root.children[0].children[7].children[0].children[0].children[4].children[0].children.Count);
        }
        [TestMethod]
        public void parsesSimpleExpression()
        {
            MiniJavaGrammar g = new MiniJavaGrammar();
            String p = "class main { public static void main() { assert(1+2*3); } }";
            SyntaxTree t = g.parse(p);
            Assert.AreEqual(new Operator("+"), t.root.children[0].children[7].children[0].children[2].token);
            Assert.AreEqual(2, t.root.children[0].children[7].children[0].children[2].children.Count);
            Assert.AreEqual(new Operator("*"), t.root.children[0].children[7].children[0].children[2].children[1].token);
            Assert.AreEqual(3, ((IntLiteral)t.root.children[0].children[7].children[0].children[2].children[1].children[1].token).value);
        }
        [TestMethod]
        public void parsesExpressionWithExtraParenthesis()
        {
            MiniJavaGrammar g = new MiniJavaGrammar();
            String p = "class main { public static void main() { assert(((1+2*3))); } }";
            SyntaxTree t = g.parse(p);
            Assert.AreEqual(new Operator("+"), t.root.children[0].children[7].children[0].children[2].token);
            Assert.AreEqual(2, t.root.children[0].children[7].children[0].children[2].children.Count);
            Assert.AreEqual(new Operator("*"), t.root.children[0].children[7].children[0].children[2].children[1].token);
            Assert.AreEqual(3, ((IntLiteral)t.root.children[0].children[7].children[0].children[2].children[1].children[1].token).value);
        }
        [TestMethod]
        public void parsesExpressionWithActualParenthesis()
        {
            MiniJavaGrammar g = new MiniJavaGrammar();
            String p = "class main { public static void main() { assert((1+2)*3); } }";
            SyntaxTree t = g.parse(p);
            Assert.AreEqual(new Operator("*"), t.root.children[0].children[7].children[0].children[2].token);
            Assert.AreEqual(2, t.root.children[0].children[7].children[0].children[2].children.Count);
            Assert.AreEqual(new Operator("+"), t.root.children[0].children[7].children[0].children[2].children[0].token);
            Assert.AreEqual(2, ((IntLiteral)t.root.children[0].children[7].children[0].children[2].children[0].children[1].token).value);
        }
    }
}
