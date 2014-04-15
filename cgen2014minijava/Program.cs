using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cgen2014minijava
{
    class Program
    {
        static void Main(string[] args)
        {
            ASTParser p = new ASTParser();
            MiniJavaGrammar g = new MiniJavaGrammar();
            String s = @"
                class main {
                  public static void main () {
                    int[] a;
                    a = new int[10];
                    a[3] = 1;
                  }
                }
                ";
            SyntaxTree tree = g.parse(s);
            tree.DebugPrint();
            ProgramNode t = p.parse(tree);
            System.Console.ReadLine();
        }
    }
}
