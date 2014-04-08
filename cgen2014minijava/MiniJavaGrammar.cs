using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace comp2014minipl
{
    public class MiniPLGrammar
    {
        private Decommenter decommenter;
        private Parser parser;
        private Scanner scanner;
        public Dictionary<String, Token> t;
        public Dictionary<String, Token> o;
        public MiniPLGrammar()
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
            addKeywordOrNonTerminal("for");
            addKeywordOrNonTerminal("in");
            addKeywordOrNonTerminal("..", "\\.\\.");
            addKeywordOrNonTerminal("do");
            addKeywordOrNonTerminal("end");
            addKeywordOrNonTerminal("var");
            addKeywordOrNonTerminal(";");
            addKeywordOrNonTerminal(":");
            addKeywordOrNonTerminal(":=");
            addKeywordOrNonTerminal("read");
            addKeywordOrNonTerminal("print");
            addKeywordOrNonTerminal("assert");
            addKeywordOrNonTerminal("type");
            addKeywordOrNonTerminal("int");
            addKeywordOrNonTerminal("string");
            addKeywordOrNonTerminal("bool");
            addKeywordOrNonTerminal("(", "\\(");
            addKeywordOrNonTerminal(")", "\\)");
            addKeywordOrNonTerminal("prog");
            addKeywordOrNonTerminal("stmts");
            addKeywordOrNonTerminal("stmt");
            addKeywordOrNonTerminal("expr");
            addKeywordOrNonTerminal("expr2");
            addKeywordOrNonTerminal("expr3");
            addKeywordOrNonTerminal("opnd");
            addKeywordOrNonTerminal("op");
            addKeywordOrNonTerminal("unary_op");
            addKeywordOrNonTerminal("maybeAssign");

            addOperator("+");
            addOperator("-");
            addOperator("*");
            addOperator("/");
            addOperator("<");
            addOperator("=");
            addOperator("&");
            //unary must be handled separately from binary because otherwise "abc"!"abc" is parseable
            Token not = new Operator("!");
            o.Add("!", not);
            scanner.addOperator("!");

            Token id = new Identifier("");
            Token intV = new IntLiteral("0");
            Token strV = new StringLiteral("");
            Token boolV = new BoolLiteral("");

            parser.addProduction(t["prog"], new List<Token> { t["stmt"], t[";"], t["stmts"] });
            parser.addProduction(t["stmts"], new List<Token> { t["stmt"], t[";"], t["stmts"] });
            parser.addProduction(t["stmts"], new List<Token> { });
            parser.addProduction(t["stmt"], new List<Token> { t["var"], id, t[":"], t["type"], t["maybeAssign"] });
            parser.addProduction(t["stmt"], new List<Token> { t["for"], id, t["in"], t["expr"], t[".."], t["expr"], t["do"], t["stmt"], t[";"], t["stmts"], t["end"], t["for"] });
            parser.addProduction(t["stmt"], new List<Token> { id, t[":="], t["expr"] });
            parser.addProduction(t["stmt"], new List<Token> { t["read"], id });
            parser.addProduction(t["stmt"], new List<Token> { t["print"], t["expr"] });
            parser.addProduction(t["stmt"], new List<Token> { t["assert"], t["("], t["expr"], t[")"] });
            parser.addProduction(t["expr"], new List<Token> { t["expr2"] });
            parser.addProduction(t["expr"], new List<Token> { t["unary_op"], t["opnd"] });
            parser.addProduction(t["expr2"], new List<Token> { t["opnd"], t["expr3"] });
            parser.addProduction(t["expr3"], new List<Token> { t["op"], t["opnd"] });
            parser.addProduction(t["expr3"], new List<Token> { });
            parser.addProduction(t["opnd"], new List<Token> { intV });
            parser.addProduction(t["opnd"], new List<Token> { strV });
            parser.addProduction(t["opnd"], new List<Token> { boolV });
            parser.addProduction(t["opnd"], new List<Token> { id });
            parser.addProduction(t["opnd"], new List<Token> { t["("], t["expr"], t[")"] });
            parser.addProduction(t["type"], new List<Token> { t["int"] });
            parser.addProduction(t["type"], new List<Token> { t["string"] });
            parser.addProduction(t["type"], new List<Token> { t["bool"] });
            parser.addProduction(t["maybeAssign"], new List<Token> { t[":="], t["expr"] });
            parser.addProduction(t["maybeAssign"], new List<Token> { });
            parser.addProduction(t["unary_op"], new List<Token> { not });
            parser.addProduction(t["unary_op"], new List<Token> { o["-"] });

            parser.setStartSymbol(t["prog"]);
            parser.prepareForParsing();
        }
        private void addOperator(String op)
        {
            o.Add(op, new Operator(op));
            scanner.addOperator(op);
            parser.addProduction(t["op"], new List<Token>{ o[op] });
        }
        private void addKeywordOrNonTerminal(String keyword, String regex = null)
        {
            if (regex == null)
            {
                regex = keyword;
            }
            t.Add(keyword, new Keyword(keyword));
            scanner.addKeyword(regex);
        }
    }
}
