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
            String p = "class main { public static void main() { assert(a.b(ab[d.c(1, 2)], d())+5*a.f()()); } }";
            SyntaxTree t = g.parse(p);
            System.Console.ReadLine();
        }
    }
}
