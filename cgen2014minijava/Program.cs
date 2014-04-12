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
            SyntaxTree tree = g.parse(s);
            tree.DebugPrint();
            ProgramNode t = p.parse(tree);
            System.Console.ReadLine();
        }
    }
}
