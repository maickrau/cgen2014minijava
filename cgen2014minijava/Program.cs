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
            SyntaxTree tree = null;
            try
            {
                tree = g.parse(s);
            }
            catch (ScannerException e)
            {
                System.Console.WriteLine("Errors parsing the program: " + e);
            }
            catch (ParserException e)
            {
                System.Console.WriteLine("Errors parsing the program: " + e);
                return;
            }

            System.Console.WriteLine("Semantic analysis");
            ProgramNode t = null;
            try
            {
                t = p.parse(tree);
            }
            catch (SemanticError e)
            {
                System.Console.WriteLine("Semantic errors in program: " + e);
                return;
            }

            System.Console.WriteLine("Code generation");
            CodeGenerator cg = new CodeGenerator();
            //no try-catch here because code generation exceptions are bugs in this program
            //any errors in the input program should have been noticed at syntax/semantic analysis
            AssemblyBuilder a = cg.generateModule(t, args[1]);

            System.Console.WriteLine("Saving generated executable");
            a.Save(args[1]);
            System.Console.WriteLine("Done");
        }
    }
}
