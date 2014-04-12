using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using cgen2014minijava;

namespace cgen2014minijavaTest
{
    [TestClass]
    public class ASTTest
    {
        [TestMethod]
        public void ParsesClasses()
        {
            ASTParser p = new ASTParser();
            MiniJavaGrammar g = new MiniJavaGrammar();
            String s = "class main { public static void main() { } } class other { }";
            ProgramNode t = p.parse(g.parse(s));
            Assert.AreEqual("main", t.classes[0].name);
            Assert.AreEqual("main", t.mainClass.name);
            Assert.AreEqual("other", t.classes[1].name);
        }
        [TestMethod]
        public void parsesStatements()
        {
            ASTParser p = new ASTParser();
            MiniJavaGrammar g = new MiniJavaGrammar();
            String s = @"
                class main 
                { 
                public static void main() 
                { 
                    assert(true);
                    System.out.println(5);
                    if (1)
                        if (2)
                            System.out.println(1);
                        else
                            System.out.println(0);
                    while(false)
                    {
                        System.out.println(0);
                    }
                    {
                        int a;
                        a = 1;
                    }
                }
                }";
            ProgramNode t = p.parse(g.parse(s));
            Assert.AreEqual(5, t.mainClass.methods[0].statements.statements.Count);
            Assert.IsInstanceOfType(t.mainClass.methods[0].statements.statements[0], typeof(AssertNode));
            Assert.IsInstanceOfType(t.mainClass.methods[0].statements.statements[1], typeof(PrintNode));
            Assert.IsInstanceOfType(t.mainClass.methods[0].statements.statements[2], typeof(IfNode));
            Assert.IsInstanceOfType(((IfNode)t.mainClass.methods[0].statements.statements[2]).thenNode, typeof(IfNode));
            Assert.AreNotEqual(null, ((IfNode)((IfNode)t.mainClass.methods[0].statements.statements[2]).thenNode).elseNode);
            Assert.AreEqual(null, ((IfNode)t.mainClass.methods[0].statements.statements[2]).elseNode);
            Assert.IsInstanceOfType(t.mainClass.methods[0].statements.statements[3], typeof(WhileNode));
            Assert.IsInstanceOfType(t.mainClass.methods[0].statements.statements[4], typeof(BlockStatementNode));
            Assert.AreEqual(1, ((BlockStatementNode)t.mainClass.methods[0].statements.statements[4]).statements.Count);
            Assert.AreEqual(1, ((BlockStatementNode)t.mainClass.methods[0].statements.statements[4]).locals.Count);
            Assert.IsInstanceOfType(((BlockStatementNode)t.mainClass.methods[0].statements.statements[4]).statements[0], typeof(AssignmentNode));
            Assert.AreEqual("a", ((BlockStatementNode)t.mainClass.methods[0].statements.statements[4]).locals[0].name);
        }
        [TestMethod]
        public void parsesFormals()
        {
            ASTParser p = new ASTParser();
            MiniJavaGrammar g = new MiniJavaGrammar();
            String s = @"
                class main 
                { 
                    public static void main() 
                    { 
                    }
                }
                class second
                {
                    public int f(int a, int b, int c) { }
                }
";
            ProgramNode t = p.parse(g.parse(s));
            Assert.AreEqual(3, t.classes[1].methods[0].arguments.Count);
            Assert.AreEqual("a", t.classes[1].methods[0].arguments[0].name);
            Assert.AreEqual("b", t.classes[1].methods[0].arguments[1].name);
            Assert.AreEqual("c", t.classes[1].methods[0].arguments[2].name);
        }
        [TestMethod]
        public void parsesMemberVariables()
        {
            ASTParser p = new ASTParser();
            MiniJavaGrammar g = new MiniJavaGrammar();
            String s = @"
                class main 
                { 
                    public static void main() 
                    { 
                    }
                }
                class second
                {
                    int a;
                    int b;
                }
";
            ProgramNode t = p.parse(g.parse(s));
            Assert.AreEqual(2, t.classes[1].members.Count);
            Assert.AreEqual("a", t.classes[1].members[0].name);
            Assert.AreEqual("b", t.classes[1].members[1].name);
        }
        [TestMethod]
        public void parsesMethodCall()
        {
            ASTParser p = new ASTParser();
            MiniJavaGrammar g = new MiniJavaGrammar();
            String s = @"
                class main 
                { 
                    public static void main() 
                    { 
                        f(a, 2, 3);
                    }
                }
";
            ProgramNode t = p.parse(g.parse(s));
            Assert.IsInstanceOfType(t.mainClass.methods[0].statements.statements[0], typeof(FunctionCall));
            Assert.AreEqual(3, ((FunctionCall)t.mainClass.methods[0].statements.statements[0]).args.Count);
            Assert.AreEqual("a", ((LocalOrMemberReference)((FunctionCall)t.mainClass.methods[0].statements.statements[0]).args[0]).var.name);
            Assert.AreEqual(2, ((IntConstant)((FunctionCall)t.mainClass.methods[0].statements.statements[0]).args[1]).value);
            Assert.AreEqual(3, ((IntConstant)((FunctionCall)t.mainClass.methods[0].statements.statements[0]).args[2]).value);
        }
        [TestMethod]
        public void parsesThis()
        {
            ASTParser p = new ASTParser();
            MiniJavaGrammar g = new MiniJavaGrammar();
            String s = @"
                class main 
                { 
                    public static void main() 
                    { 
                        this.f(a, 2, 3);
                        this.a = 1;
                        b = this;
                    }
                }
";
            ProgramNode t = p.parse(g.parse(s));
            Assert.AreEqual(3, t.mainClass.methods[0].statements.statements.Count);
            Assert.IsInstanceOfType(t.mainClass.methods[0].statements.statements[0], typeof(FunctionCall));
            Assert.IsInstanceOfType(t.mainClass.methods[0].statements.statements[1], typeof(AssignmentNode));
            Assert.IsInstanceOfType(t.mainClass.methods[0].statements.statements[2], typeof(AssignmentNode));
            Assert.IsInstanceOfType(((FunctionCall)t.mainClass.methods[0].statements.statements[0]).f.obj, typeof(ThisNode));
            Assert.IsInstanceOfType(((AssignmentNode)t.mainClass.methods[0].statements.statements[1]).target, typeof(ObjectMemberReference));
            Assert.IsInstanceOfType(((ObjectMemberReference)((AssignmentNode)t.mainClass.methods[0].statements.statements[1]).target).obj, typeof(ThisNode));
            Assert.IsInstanceOfType(((AssignmentNode)t.mainClass.methods[0].statements.statements[2]).value, typeof(ThisNode));
        }
        [TestMethod]
        public void parsesSampleProgram()
        {
            ASTParser p = new ASTParser();
            MiniJavaGrammar g = new MiniJavaGrammar();
            String s = @"
                class Factorial {
                  public static void main () {
                    System.out.println (new Fac ().ComputeFac (10));
                  }
                }
                class Fac {
                  public int ComputeFac (int num) {
                    assert (num > -1);
                    int num_aux;
                    if (num == 0)
                      num_aux = 1;
                    else 
                      num_aux = num * this.ComputeFac (num-1);
                    return num_aux;
                  }
                }        
                ";
            ProgramNode t = p.parse(g.parse(s));
            Assert.AreEqual(2, t.classes.Count);
            Assert.AreEqual("Factorial", t.classes[0].name);
            Assert.IsInstanceOfType(t.classes[0].methods[0].statements.statements[0], typeof(PrintNode));
            PrintNode printexpr = (PrintNode)t.classes[0].methods[0].statements.statements[0];
            Assert.IsInstanceOfType(printexpr.value, typeof(FunctionCall));
            Assert.IsInstanceOfType(((FunctionCall)printexpr.value).f, typeof(ObjectMethodReference));
            Assert.IsInstanceOfType(((ObjectMethodReference)((FunctionCall)printexpr.value).f).obj, typeof(NewSingular));
            Assert.AreEqual("Fac", t.classes[1].name);
            Assert.AreEqual(3, t.classes[1].methods[0].statements.statements.Count);
            Assert.AreEqual(1, t.classes[1].methods[0].statements.locals.Count);
            Assert.AreEqual("num_aux", t.classes[1].methods[0].statements.locals[0].name);
        }
    }
}
