using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.IO;

namespace cgen2014minijava
{
    class Program
    {
        static String getProgramFromFile(String fileName)
        {
            StringBuilder str = new StringBuilder();
            using (StreamReader reader = new StreamReader(fileName))
            {
                String line = reader.ReadLine();
                while (line != null)
                {
                    str.AppendLine(line);
                    line = reader.ReadLine();
                }
            }
            return str.ToString();
        }
        static void Main(string[] args)
        {
            if (args.Count() != 2)
            {
                System.Console.WriteLine("Usage: cgen2014minijava.exe minijavaSourceFileName outputFileName");
                return;
            }

            System.Console.WriteLine("Reading source file");
            String s = getProgramFromFile(args[0]);

            System.Console.WriteLine("Syntax analysis");
            ASTParser p = new ASTParser();
            MiniJavaGrammar g = new MiniJavaGrammar();
            SyntaxTree tree = g.parse(s);

            System.Console.WriteLine("Semantic analysis");
            ProgramNode t = p.parse(tree);

            System.Console.WriteLine("Code generation");
            CodeGenerator cg = new CodeGenerator();
            AssemblyBuilder a = cg.generateModule(t, args[1]);

            System.Console.WriteLine("Saving generated executable");
            a.Save(args[1]);
            System.Console.WriteLine("Done");
        }
    }
}
