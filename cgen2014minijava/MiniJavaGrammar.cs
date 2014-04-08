using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cgen2014minijava
{
    public class MiniJavaGrammar
    {
        private Decommenter decommenter;
        private Parser parser;
        private Scanner scanner;
        public Dictionary<String, Token> t;
        public Dictionary<String, Token> o;
        public MiniJavaGrammar()
        {
            decommenter = new Decommenter();
            parser = new Parser();
            scanner = new Scanner();
            t = new Dictionary<String, Token>();
            o = new Dictionary<String, Token>();
            initializeGrammar();
        }
        public SyntaxTree parse(String str)
        {
            return parser.parse(scanner.parse(decommenter.decomment(str)));
        }
        private void initializeGrammar()
        {
            addKeyword("class");
            addKeyword("public static void main"); //yuck
            addKeyword("{");
            addKeyword("}");
            addKeyword("(", "\\(");
            addKeyword(")", "\\)");
            addKeyword("[", "\\[");
            addKeyword("]", "\\]");
            addKeyword("extends");
            addKeyword("public");
            addKeyword(";");
            addKeyword("int");
            addKeyword("boolean");
            addKeyword("void");
            addKeyword("assert");
            addKeyword("if");
            addKeyword("else");
            addKeyword("while");
            addKeyword("System.out.println"); //eww
            addKeyword("return");
            addKeyword("=");
            addKeyword(".", "\\.");
            addKeyword(",");
            addKeyword("length");
            addKeyword("new");
            addKeyword("this");
            addKeyword("true");
            addKeyword("false");

            addNonTerminal("prog");
            addNonTerminal("main class");
            addNonTerminal("class decls");
            addNonTerminal("class decl");
            addNonTerminal("decl");
            addNonTerminal("decls");
            addNonTerminal("variable decl");
            addNonTerminal("method decl");
            addNonTerminal("formals");
            addNonTerminal("formal");
            addNonTerminal("more formals");
            addNonTerminal("type");
            addNonTerminal("base type");
            addNonTerminal("maybe array");
            addNonTerminal("maybe extends");
            addNonTerminal("statement");
            addNonTerminal("statements");
            addNonTerminal("op");
            addNonTerminal("statement expr");
            addNonTerminal("if statement"); //special!
            addNonTerminal("expr"); //special!

            addOperator("&&");
            addOperator("||");
            addOperator("<");
            addOperator(">");
            addOperator("==");
            addOperator("+");
            addOperator("-");
            addOperator("*");
            addOperator("/");
            addOperator("%");
            //unary must be handled separately from binary because otherwise "abc"!"abc" is parseable
            Token not = new Operator("!");
            o.Add("!", not);
            scanner.addOperator("!");

            Token id = new Identifier("");
            Token intV = new IntLiteral("0");
            Token boolV = new BoolLiteral("");

            parser.addProduction(t["prog"], new List<Token> { t["main class"], t["class decls"] });
            parser.addProduction(t["main class"], new List<Token> { t["class"], id, t["{"], t["public static void main"], t["("], t[")"], t["{"], t["statements"], t["}"], t["}"] });
            parser.addProduction(t["class decls"], new List<Token> { t["class decl"], t["class decls"] });
            parser.addProduction(t["class decls"], new List<Token> { });
            parser.addProduction(t["class decl"], new List<Token> { t["class"], id, t["maybe extends"], t["{"], t["decls"], t["}"] });
            parser.addProduction(t["decl"], new List<Token> { t["variable decl"] });
            parser.addProduction(t["decl"], new List<Token> { t["method decl"] });
            parser.addProduction(t["decls"], new List<Token> { t["decl"], t["decls"] });
            parser.addProduction(t["decls"], new List<Token> { });
            parser.addProduction(t["method decl"], new List<Token> { t["public"], t["type"], id, t["("], t["formals"], t[")"], t["{"], t["statements"], t["}"] });
            parser.addProduction(t["variable decl"], new List<Token> { t["type"], id, t[";"] });
            parser.addProduction(t["formals"], new List<Token> { });
            parser.addProduction(t["formals"], new List<Token> { t["formal"], t["more formals"] });
            parser.addProduction(t["formal"], new List<Token> { t["type"], id });
            parser.addProduction(t["more formals"], new List<Token> { t[","], t["type"], id, t["more formals"] });
            parser.addProduction(t["more formals"], new List<Token> { });
            parser.addProduction(t["type"], new List<Token> { t["base type"], t["maybe array"] });
            parser.addProduction(t["base type"], new List<Token> { t["int"] });
            parser.addProduction(t["base type"], new List<Token> { t["boolean"] });
            parser.addProduction(t["base type"], new List<Token> { t["void"] });
            parser.addProduction(t["base type"], new List<Token> { id });
            parser.addProduction(t["maybe array"], new List<Token> { t["["], t["]"] });
            parser.addProduction(t["maybe array"], new List<Token> { });
            parser.addProduction(t["maybe extends"], new List<Token> { t["extends"], id });
            parser.addProduction(t["maybe extends"], new List<Token> { });
            parser.addProduction(t["statement"], new List<Token> { t["assert"], t["("], t["expr"], t[")"], t[";"] });
//            parser.addProduction(t["statement"], new List<Token> { t["variable decl"] });
            parser.addProduction(t["statement"], new List<Token> { t["{"], t["statements"], t["}"] });
            parser.addProduction(t["statement"], new List<Token> { t["if statement"] });
            parser.addProduction(t["statement"], new List<Token> { t["while"], t["("], t["expr"], t[")"], t["statement"] });
            parser.addProduction(t["statement"], new List<Token> { t["System.out.println"], t["("], t["expr"], t[")"], t[";"] });
            parser.addProduction(t["statement"], new List<Token> { t["return"], t["expr"], t[";"] });
            parser.addProduction(t["statement"], new List<Token> { t["expr"], t["statement expr"] });
            parser.addProduction(t["statement expr"], new List<Token> { t[";"] }); //method call or variable declaration
            parser.addProduction(t["statement expr"], new List<Token> { t["="], t["expr"] }); //variable declaration can be initialized
            parser.addProduction(t["statements"], new List<Token> { t["statement"], t["statements"] });
            parser.addProduction(t["statements"], new List<Token> { });

            parser.addProduction(t["if statement"], new List<Token> { t["if"] });
            parser.addProduction(t["expr"], new List<Token> { t["int"] }); //variable declaration is parsed as an expression
            parser.addProduction(t["expr"], new List<Token> { t["boolean"] });
            parser.addProduction(t["expr"], new List<Token> { id });
            parser.addProduction(t["expr"], new List<Token> { intV });
            parser.addProduction(t["expr"], new List<Token> { boolV });
            parser.addProduction(t["expr"], new List<Token> { t["("], t["expr"], t[")"] });
            parser.addProduction(t["expr"], new List<Token> { t["new"] });
            parser.addProduction(t["expr"], new List<Token> { not });
            parser.addProduction(t["expr"], new List<Token> { o["-"] });
            parser.addProduction(t["expr"], new List<Token> { t["this"] });
            parser.addProduction(t["expr"], new List<Token> { t["true"] });
            parser.addProduction(t["expr"], new List<Token> { t["false"] });

            parser.setStartSymbol(t["prog"]);
            parser.prepareForParsing();
        }
        private void addOperator(String op)
        {
            o.Add(op, new Operator(op));
            scanner.addOperator(op);
            parser.addProduction(t["op"], new List<Token>{ o[op] });
        }
        private void addKeyword(String keyword, String regex = null)
        {
            if (regex == null)
            {
                regex = keyword;
            }
            t.Add(keyword, new Keyword(keyword));
            scanner.addKeyword(regex);
        }
        private void addNonTerminal(String keyword)
        {
            t.Add(keyword, new NonTerminal(keyword));
        }
    }
}
