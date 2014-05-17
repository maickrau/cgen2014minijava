using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using cgen2014minijava;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Collections.Generic;

namespace cgen2014minijavaTest
{
    [TestClass]
    public class CompilerTest
    {
        List<String> compileAndRun(String source)
        {
            //compile the program
            ASTParser parser = new ASTParser();
            MiniJavaGrammar g = new MiniJavaGrammar();
            SyntaxTree tree = g.parse(source);
            ProgramNode t = parser.parse(tree);
            CodeGenerator cg = new CodeGenerator();
            AssemblyBuilder a = cg.generateModule(t, "temp.exe");
            a.Save("temp.exe");

            //run the program and capture the output
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "temp.exe";
            p.Start();
            String output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            System.IO.File.Delete("temp.exe");

            output = output.Replace(Environment.NewLine, "\n"); //replace \r\n with \n so split doesn't produce extra empty lines
            output = output.Substring(0, output.Length - 1); //remove trailing endline
            return new List<String>(output.Split(new char[]{'\n'}));
        }
        [TestMethod]
        public void factorialWorks()
        {
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
}";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("3628800", output[0]);
        }
        [TestMethod]
        public void arrayAssignmentWorks()
        {
            String s = @"
class Arraystuff {
  public static void main () {
    int[] a;
    a = new int[5];
    System.out.println(a[0]);
    System.out.println(a[1]);
    a[1] = 15;
    System.out.println(a[0]);
    System.out.println(a[1]);
  }
}
";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("0", output[0]);
            Assert.AreEqual("0", output[1]);
            Assert.AreEqual("0", output[2]);
            Assert.AreEqual("15", output[3]);
        }
        [TestMethod]
        public void complexExpressionWorks()
        {
            String s = @"
class Arraystuff {
  public static void main () {
    System.out.println((1+1*5%3)/2);
  }
}
";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("1", output[0]);
        }
        [TestMethod]
        public void ifWorks()
        {
            String s = @"
class Arraystuff {
  public static void main () {
    if (true) {
        System.out.println(1);
    }
    else {
        System.out.println(2);
    }
    if (false) {
        System.out.println(3);
    }
    else {
        System.out.println(4);
    }
  }
}
";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("1", output[0]);
            Assert.AreEqual("4", output[1]);
            Assert.AreEqual(2, output.Count);
        }
        [TestMethod]
        public void comparisonOperatorsWork()
        {
            String s = @"
class Arraystuff {
  public static void main () {
if (1 < 2) {
System.out.println(1);
}
else {
System.out.println(0);
}
if (1 > 2) {
System.out.println(0);
}
else {
System.out.println(1);
}
if (1 == 1) {
System.out.println(1);
}
else {
System.out.println(0);
}
if (1 == 2) {
System.out.println(0);
}
else {
System.out.println(1);
}
  }
}
";
            List<String> output = compileAndRun(s);
            Assert.AreEqual(4, output.Count);
            foreach (String st in output) {
                Assert.AreEqual("1", st);
            }
        }
        [TestMethod]
        public void arithmeticOperatorsWork()
        {
            String s = @"
class Arraystuff {
  public static void main () {
System.out.println(5+2);
System.out.println(5-2);
System.out.println(5*2);
System.out.println(5%2);
System.out.println(5/2);
  }
}
";
            List<String> output = compileAndRun(s);
            Assert.AreEqual(5, output.Count);
            Assert.AreEqual("7", output[0]);
            Assert.AreEqual("3", output[1]);
            Assert.AreEqual("10", output[2]);
            Assert.AreEqual("1", output[3]);
            Assert.AreEqual("2", output[4]);
        }
        [TestMethod]
        public void booleanOperatorsWork()
        {
            String s = @"
class Arraystuff {
  public static void main () {
if (!true) {
System.out.println(0);
}
else
{
System.out.println(1);
}
if (!false) {
System.out.println(1);
}
else
{
System.out.println(0);
}
if (false || true) {
System.out.println(1);
}
else
{
System.out.println(0);
}
if (false || false) {
System.out.println(0);
}
else
{
System.out.println(1);
}
if (true || true) {
System.out.println(1);
}
else
{
System.out.println(0);
}
if (false && true) {
System.out.println(0);
}
else
{
System.out.println(1);
}
if (false && false) {
System.out.println(0);
}
else
{
System.out.println(1);
}
if (true && true) {
System.out.println(1);
}
else
{
System.out.println(0);
}
  }
}
";
            List<String> output = compileAndRun(s);
            Assert.AreEqual(8, output.Count);
            foreach (String st in output)
            {
                Assert.AreEqual("1", st);
            }
        }
        [TestMethod]
        public void objectArraysWork()
        {
            String s = @"
class Arraystuff {
  public static void main () {
    Fac[] a;
    a = new Fac[5];
    a[1] = new Fac();
    a[3] = new Fac();
    System.out.println(a[1].ComputeFac(3));
    System.out.println(a[3].ComputeFac(5));
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
}";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("6", output[0]);
            Assert.AreEqual("120", output[1]);
        }
        [TestMethod]
        public void functionsWork()
        {
            String s = @"
class Arraystuff {
  public static void main () {
    Fac a;
    a = new Fac();
    System.out.println(a.ComputeFac(5));
  }
}
class Fac {
  public int ComputeFac (int num) {
    return identityPlusOne(num-1);
  }
    public int identityPlusOne(int num) {
        return num+1;
    }
}";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("5", output[0]);
        }
        [TestMethod]
        public void membersWork()
        {
            String s = @"
class Arraystuff {
  public static void main () {
    Fac a;
    a = new Fac();
    System.out.println(a.getA());
    a.setA(5);
    System.out.println(a.getA());
    a.setA(15);
    System.out.println(a.getA());
  }
}
class Fac {
    int a;
public int getA() {
    return a;
}
public void setA(int num) {
    a = num;
}
}";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("0", output[0]);
            Assert.AreEqual("5", output[1]);
            Assert.AreEqual("15", output[2]);
        }
        [TestMethod]
        public void whileWorks()
        {
            String s = @"
class Arraystuff {
  public static void main () {
    int a;
    a = 3;
    while (a > 5) {
        System.out.println(0);
    }
    while (a < 8) {
        System.out.println(a);
        a = a+1;
    }
  }
}
";
            List<String> output = compileAndRun(s);
            Assert.AreEqual(5, output.Count);
            Assert.AreEqual("3", output[0]);
            Assert.AreEqual("4", output[1]);
            Assert.AreEqual("5", output[2]);
            Assert.AreEqual("6", output[3]);
            Assert.AreEqual("7", output[4]);
        }
        [TestMethod]
        public void inheritanceWorks()
        {
            String s = @"
class Factorial {
  public static void main () {
    System.out.println (new Fac2 ().ComputeFac (10));
  }
}
class Fac2 extends Fac { }
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
}";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("3628800", output[0]);
        }
        [TestMethod]
        public void functionOverridingWorks()
        {
            String s = @"
class Factorial {
  public static void main () {
    System.out.println (new Fac ().getNum ());
    System.out.println (new Fac2 ().getNum ());
  }
}
class Fac2 extends Fac {
public int getNum() {
return 10;
}
}
class Fac {
  public int getNum () {
    return 5;
  }
}";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("5", output[0]);
            Assert.AreEqual("10", output[1]);
        }
        [TestMethod]
        public void localsShadowMembers()
        {
            String s = @"
class Factorial {
  public static void main () {
    Fac b;
    b = new Fac();
    System.out.println (b.getMember());
    System.out.println (b.getLocal());
    b.setMember(10);
    System.out.println (b.getMember());
    System.out.println (b.getLocal());
    b.setLocal(5);
    System.out.println (b.getMember());
    System.out.println (b.getLocal());
  }
}
class Fac {
int a;
public int getMember() {
return a;
}
public int getLocal() {
int a;
return a;
}
public void setMember(int a) {
this.a = a;
}
public void setLocal(int num) {
int a;
a = num;
}
}";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("0", output[0]);
            Assert.AreEqual("0", output[1]);
            Assert.AreEqual("10", output[2]);
            Assert.AreEqual("0", output[3]);
            Assert.AreEqual("10", output[4]);
            Assert.AreEqual("0", output[5]);
        }
        [TestMethod]
        public void voidFunctionCallsWork()
        {
            String s = @"
class Factorial {
  public static void main () {
    Fac a;
    a = new Fac();
    a.print5();
    a.print10();
    System.out.println(1+1); //a.print10() shouldn't leave anything on stack
  }
}
class Fac {
public int print10() {
System.out.println(10);
return 1;
}
public void print5() {
System.out.println(5);
}
}";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("5", output[0]);
            Assert.AreEqual("10", output[1]);
            Assert.AreEqual("2", output[2]);
        }
        [TestMethod]
        public void membersAreNotStatic()
        {
            String s = @"
class Factorial {
  public static void main () {
    Fac b;
    Fac c;
    b = new Fac();
    c = new Fac();
    System.out.println (b.getA());
    System.out.println (c.getA());
    b.setA(10);
    System.out.println (b.getA());
    System.out.println (c.getA());
  }
}
class Fac {
int a;
public int getA() {
return a;
}
public void setA(int a) {
this.a = a;
}
}";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("0", output[0]);
            Assert.AreEqual("0", output[1]);
            Assert.AreEqual("10", output[2]);
            Assert.AreEqual("0", output[3]);
        }
        [TestMethod]
        public void subclassCanBeAssignedToSuperclass()
        {
            String s = @"
class Factorial {
  public static void main () {
    Fac b;
    b = new Fac2();
    System.out.println(b.get());
  }
}
class Fac2 extends Fac {}
class Fac {
public int get() { return 5; }
}";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("5", output[0]);
        }
        [TestMethod]
        public void dynamicDispatchWorks()
        {
            String s = @"
class Factorial {
  public static void main () {
    Fac b;
    b = new Fac2();
    System.out.println(b.f());
    System.out.println(b.g());
    b = new Fac();
    System.out.println(b.f());
    System.out.println(b.g());
  }
}
class Fac2 extends Fac {
public int g() { return 2; }
}
class Fac {
public int f() { return g(); }
public int g() { return 1; }
}";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("2", output[0]);
            Assert.AreEqual("2", output[1]);
            Assert.AreEqual("1", output[2]);
            Assert.AreEqual("1", output[3]);
        }
        [TestMethod]
        public void testPolymorphismInParameters()
        {
            String s = @"
class Factorial {
  public static void main () {
    Fac b;
    b = new Fac2();
    FacCaller c;
    c = new FacCaller();
    System.out.println(c.call(b));
    b = new Fac();
    System.out.println(c.call(b));
  }
}
class Fac2 extends Fac {
public int g() { return 2; }
}
class Fac {
public int f() { return g(); }
public int g() { return 1; }
}
class FacCaller {
public int call(Fac a) {
return a.f();
}
}
";
            List<String> output = compileAndRun(s);
            Assert.AreEqual("2", output[0]);
            Assert.AreEqual("1", output[1]);
        }
    }
}
