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
            MiniJavaGrammar g = new MiniJavaGrammar();
            String p = "class main { public static void main() { assert(((1+2*3))); } }";
            SyntaxTree t = g.parse(p);
            System.Console.ReadLine();
        }
    }
}
