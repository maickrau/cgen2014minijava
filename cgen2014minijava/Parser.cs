using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cgen2014minijava
{
    public class ParserException : Exception
    {
        public ParserException(String str) : base(str) { }
        public ParserException() : base() { }
    }
    public class ParseError : ParserException
    {
        public ParseError(String str) : base(str) { }
        public ParseError(List<String> errors) : base(parseErrors(errors)) { }

        private static String parseErrors(List<String> errors)
        {
            StringBuilder sb = new StringBuilder();
            foreach (String s in errors)
            {
                sb.AppendLine(s);
            }
            return sb.ToString();
        }
    }
    public class ParserEofReachedException : ParserException
    {
        public ParserEofReachedException() : base() { }
    }
    public class NonTerminal : Token
    {
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (!(obj is NonTerminal))
            {
                return false;
            }
            return value == ((NonTerminal)obj).value;
        }
        public NonTerminal(String name)
        {
            value = name;
        }
        public override String ToString()
        {
            return "Nonterminal:\"" + value + "\"";
        }
        String value;
    }
    public class SyntaxTree
    {
        public SyntaxNode root;
        public void DebugPrint()
        {
            root.debugPrint(0);
        }
        public SyntaxTree(SyntaxNode root)
        {
            this.root = root;
        }
    }
    public class SyntaxNode
    {
        public int line;
        public int position;
        public Token token;
        public SyntaxNode parent;
        public List<SyntaxNode> children;
        public void debugPrint(int depth)
        {
            for (int i = 0; i < depth; i++)
            {
                System.Console.Write(" ");
            }
            System.Console.WriteLine(token);
            foreach (SyntaxNode n in children)
            {
                n.debugPrint(depth + 1);
            }
        }
        public SyntaxNode(Token token)
        {
            this.token = token;
            children = new List<SyntaxNode>();
        }
    }
    public class Parser
    {
        Dictionary<Token, List<List<Token>>> productionRules;
        Dictionary<Tuple<Token, Token>, List<Token>> predict;
        HashSet<Token> languageTerminals;
        HashSet<Token> languageSymbols;
        HashSet<Token> expressionTokens; //all terminals an expression may contain
        Token startSymbol;
        Token eof;
        List<String> errors;
        bool eofErroredOnce;
        public void DebugPrint()
        {
            System.Console.WriteLine("Symbols:");
            foreach (Token t in languageSymbols)
            {
                System.Console.WriteLine(t);
            }
            System.Console.WriteLine("Terminals:");
            foreach (Token t in languageTerminals)
            {
                System.Console.WriteLine(t);
            }
            System.Console.WriteLine("Productions:");
            foreach (Token t in languageSymbols)
            {
                foreach (List<Token> p in productionRules[t])
                {
                    System.Console.Write("{0} =>", t);
                    foreach (Token t2 in p)
                    {
                        System.Console.Write(" {0}", t2);
                    }
                    System.Console.WriteLine();
                }
            }
            System.Console.WriteLine("Predict:");
            foreach (KeyValuePair<Tuple<Token, Token>, List<Token>> p in predict)
            {
                System.Console.Write("{0}, {1} =>", p.Key.Item1, p.Key.Item2);
                foreach (Token t in p.Value)
                {
                    System.Console.Write(" {0}", t);
                }
                System.Console.WriteLine();
            }
        }
        public Parser()
        {
            expressionTokens = new HashSet<Token>();
            productionRules = new Dictionary<Token, List<List<Token>>>();
            languageSymbols = new HashSet<Token>();
            languageTerminals = new HashSet<Token>();
            predict = new Dictionary<Tuple<Token, Token>, List<Token>>();
            eof = new NonTerminal("eof");
            addSymbol(eof);
            addSymbol(new NonTerminal("expr"));
        }
        public void addExpressionTerminal(Token s)
        {
            addSymbol(s);
            expressionTokens.Add(s);
            productionRules[new NonTerminal("expr")].Add(new List<Token>{s});
        }
        private void addSymbol(Token s)
        {
            languageSymbols.Add(s);
            if (!productionRules.Keys.Contains(s))
            {
                productionRules[s] = new List<List<Token>>();
            }
        }
        public void addProduction(Token token, List<Token> products)
        {
            addSymbol(token);
            foreach (Token t in products)
            {
                addSymbol(t);
            }
            productionRules[token].Add(new List<Token>(products));
        }
        public void setStartSymbol(Token symbol)
        {
            startSymbol = symbol;
        }
        private void calculateTerminals()
        {
            languageTerminals = new HashSet<Token>();
            foreach (Token t in languageSymbols)
            {
                if (productionRules[t].Count == 0)
                {
                    languageTerminals.Add(t);
                }
            }
        }
        private void calculatePredict()
        {
            predict = new Dictionary<Tuple<Token, Token>, List<Token>>();
            Dictionary<Token, HashSet<Token>> first = new Dictionary<Token, HashSet<Token>>();
            Dictionary<Token, HashSet<Token>> follow = new Dictionary<Token, HashSet<Token>>();
            HashSet<Token> epsilonables = new HashSet<Token>();
            foreach (Token t in languageSymbols)
            {
                first[t] = new HashSet<Token>();
                follow[t] = new HashSet<Token>();
            }
            follow[startSymbol].Add(eof);
            foreach (Token t in languageTerminals)
            {
                first[t].Add(t);
            }
            //calculate a => something (only one derivation)
            foreach (Token t in productionRules.Keys)
            {
                foreach (List<Token> p in productionRules[t])
                {
                    if (p.Count > 0)
                    {
                        first[t].Add(p[0]);
                    }
                    else
                    {
                        epsilonables.Add(t);
                    }
                }
            }
            //calculate all a =>* epsilon
            bool added = true;
            while (added)
            {
                added = false;
                foreach (Token t in productionRules.Keys)
                {
                    if (!epsilonables.Contains(t))
                    {
                        foreach (List<Token> p in productionRules[t])
                        {
                            bool thisEpsilons = true;
                            foreach (Token t2 in p)
                            {
                                if (!epsilonables.Contains(t2))
                                {
                                    thisEpsilons = false;
                                    break;
                                }
                            }
                            if (thisEpsilons)
                            {
                                epsilonables.Add(t);
                                added = true;
                            }
                        }
                    }
                }
            }
            //calculate first and follow
            added = true;
            while (added)
            {
                added = false;
                Dictionary<Token, int> oldFirstSize = new Dictionary<Token, int>();
                Dictionary<Token, int> oldFollowSize = new Dictionary<Token, int>();
                foreach (Token t in languageSymbols)
                {
                    oldFirstSize[t] = first[t].Count;
                    oldFollowSize[t] = follow[t].Count;
                }
                foreach (Token t in languageSymbols)
                {
                    HashSet<Token> newSymbols = new HashSet<Token>();
                    foreach (Token t2 in first[t])
                    {
                        newSymbols.UnionWith(first[t2]);
                    }
                    first[t].UnionWith(newSymbols);
                    foreach (List<Token> p in productionRules[t])
                    {
                        for (int i = 0; i < p.Count-1; i++)
                        {
                            follow[p[i]].UnionWith(first[p[i + 1]]);
                        }
                        for (int i = p.Count - 1; i > 0; i--)
                        {
                            if (!epsilonables.Contains(p[i]))
                            {
                                break;
                            }
                            follow[p[i-1]].UnionWith(follow[t]);
                        }
                        if (p.Count > 0)
                        {
                            follow[p[p.Count - 1]].UnionWith(follow[t]);
                        }
                    }
                }
                foreach (Token t in languageSymbols)
                {
                    if (first[t].Count != oldFirstSize[t])
                    {
                        added = true;
                        break;
                    }
                    if (follow[t].Count != oldFollowSize[t])
                    {
                        added = true;
                        break;
                    }
                }
            }
            //remove nonterminals from first, follow
            foreach (Token t in languageSymbols)
            {
                HashSet<Token> newFirst = new HashSet<Token>();
                foreach (Token f in first[t])
                {
                    if (languageTerminals.Contains(f))
                    {
                        newFirst.Add(f);
                    }
                }
                first[t] = newFirst;
                HashSet<Token> newFollow = new HashSet<Token>();
                foreach (Token f in follow[t])
                {
                    if (languageTerminals.Contains(f))
                    {
                        newFollow.Add(f);
                    }
                }
                follow[t] = newFollow;
            }
            //calculate predict
            foreach (Token t in languageSymbols)
            {
                foreach (List<Token> p in productionRules[t])
                {
                    bool allEpsilons = true;
                    for (int i = 0; i < p.Count; i++)
                    {
                        foreach (Token t2 in first[p[i]])
                        {
                            Tuple<Token, Token> key = new Tuple<Token, Token>(t, t2);
                            if (predict.ContainsKey(key) && predict[key] != p)
                            {
                                String old = "";
                                foreach (Token s in predict[key])
                                {
                                    old += s + " ";
                                }
                                String newS = "";
                                foreach (Token s in p)
                                {
                                    newS += s + " ";
                                }
                                throw new ParserException("Parser: Grammar is not LL-1, predict(" + t + "," + t2 + ") happens at least twice (" + old + " and " + newS + ")");
                            }
                            predict[key] = p;
                            if (!epsilonables.Contains(t2))
                            {
                                allEpsilons = false;
                                i = p.Count;
                            }
                        }
                    }
                    if (allEpsilons)
                    {
                        foreach (Token f in follow[t])
                        {
                            Tuple<Token, Token> key = new Tuple<Token, Token>(t, f);
                            if (predict.ContainsKey(key) && predict[key] != p)
                            {
                                String old = "";
                                foreach (Token s in predict[key])
                                {
                                    old += s + " ";
                                }
                                String newS = "";
                                foreach (Token s in p)
                                {
                                    newS += s + " ";
                                }
                                throw new ParserException("Parser: Grammar is not LL-1, predict(" + t + "," + f + ") happens at least twice (" + old + " and " + newS + ")");
                            }
                            predict[key] = p;
                        }
                    }
                }
            }
        }
        public void prepareForParsing()
        {
            if (startSymbol == null)
            {
                throw new ParserException("Parser needs to have start symbol before preparing to parse");
            }
            calculateTerminals();
            calculatePredict();
        }
        private void addError(Token location, String message)
        {
            errors.Add("" + location.line + ":" + location.position + " Syntax error: " + message);
        }
        private void addError(Token location, Token received, List<Token> expected)
        {
            if (received == eof)
            {
                if (eofErroredOnce)
                {
                    return;
                }
                eofErroredOnce = true;
            }
            StringBuilder sb = new StringBuilder();
            foreach (Token t in expected)
            {
                sb.Append(t).Append(", ");
            }
            errors.Add("" + location.line + ":" + location.position + " Syntax error: received " + received + ", expected one of " + sb.ToString());
        }
        private Token getPredictToken(List<Token> tokens, int loc)
        {
            Token predictToken;
            if (loc < tokens.Count)
            {
                predictToken = tokens[loc];
            }
            else
            {
                predictToken = eof;
            }
            if (predictToken is Identifier)
            {
                predictToken = new Identifier(predictToken, "");
            }
            if (predictToken is IntLiteral)
            {
                predictToken = new IntLiteral(predictToken, "0");
            }
            if (predictToken is StringLiteral)
            {
                predictToken = new StringLiteral(predictToken, "");
            }
            if (predictToken is BoolLiteral)
            {
                predictToken = new BoolLiteral(predictToken, "");
            }
            return predictToken;
        }
        private void parseIf(SyntaxNode currentNode, List<Token> tokens, ref int loc)
        {
            currentNode.children = new List<SyntaxNode>();
            //if
            SyntaxNode child = new SyntaxNode(new Keyword("if"));
            child.parent = currentNode;
            parse(child, tokens, ref loc);
            currentNode.children.Add(child);
            //(
            child = new SyntaxNode(new Keyword("("));
            child.parent = currentNode;
            parse(child, tokens, ref loc);
            currentNode.children.Add(child);
            //expr
            child = new SyntaxNode(new NonTerminal("expr"));
            child.parent = currentNode;
            parse(child, tokens, ref loc);
            currentNode.children.Add(child);
            //)
            child = new SyntaxNode(new Keyword(")"));
            child.parent = currentNode;
            parse(child, tokens, ref loc);
            currentNode.children.Add(child);
            //stmt
            child = new SyntaxNode(new NonTerminal("statement"));
            child.parent = currentNode;
            parse(child, tokens, ref loc);
            currentNode.children.Add(child);
            //else?
            if (getPredictToken(tokens, loc).Equals(new Keyword("else")))
            {
                loc++;
                child = new SyntaxNode(new NonTerminal("statement"));
                child.parent = currentNode;
                parse(child, tokens, ref loc);
                currentNode.children.Add(child);
            }
        }
        private void parseExpr(SyntaxNode currentNode, List<Token> tokens, ref int loc)
        {
            System.Console.WriteLine("parse expr: " + currentNode.token);
            List<Token> thisExpression = new List<Token>();
            int start = loc;
            Token t = getPredictToken(tokens, loc);
            int parenthesisDepth = 0;
            while (expressionTokens.Contains(t))
            {
                if (t.Equals(new Keyword("(")))
                {
                    parenthesisDepth++;
                }
                if (t.Equals(new Keyword(")")))
                {
                    if (parenthesisDepth == 0)
                    {
                        break;
                    }
                    parenthesisDepth--;
                }
                thisExpression.Add(t);
                loc++;
                t = getPredictToken(tokens, loc);
            }
            List<Token> realTokens = tokens.GetRange(start, loc - start);
            if (thisExpression.Count == 2)
            {
                if (realTokens[1] is Identifier)
                {
                    if (realTokens[0] is Identifier || realTokens[0].Equals(new Keyword("int")) || realTokens[0].Equals(new Keyword("bool")))
                    {
                        //variable declaration
                        currentNode.token = new NonTerminal("variable decl");
                        currentNode.token.position = realTokens[0].position;
                        currentNode.token.line = realTokens[0].line;
                        SyntaxNode typeNode = new SyntaxNode(realTokens[0]);
                        typeNode.token = new NonTerminal("type");
                        typeNode.parent = currentNode;
                        currentNode.children.Add(typeNode);
                        SyntaxNode type = new SyntaxNode(realTokens[0]);
                        type.parent = typeNode;
                        typeNode.children.Add(type);
                        SyntaxNode maybeArray = new SyntaxNode(realTokens[0]);
                        maybeArray.token = new NonTerminal("maybe array");
                        maybeArray.parent = typeNode;
                        typeNode.children.Add(maybeArray);
                        SyntaxNode id = new SyntaxNode(realTokens[1]);
                        id.parent = currentNode;
                        currentNode.children.Add(id);
                        return;
                    }
                }
            }
            SyntaxNode newNode = new SyntaxNode(tokens[0]);
            newNode.parent = currentNode;
            currentNode.children.Add(newNode);
            parseExprInternal(newNode, thisExpression, realTokens);
        }
        private void parseExprInternal(SyntaxNode currentNode, List<Token> expression, List<Token> realTokens)
        {
            
            StringBuilder sb = new StringBuilder();
            foreach (Token e in realTokens)
            {
                sb.Append(e).Append(", ");
            }
            System.Console.WriteLine("Parseexpr " + sb.ToString());
            
            if (expression.Count == 0)
            {
                throw new Exception("Something went wrong! Expression has 0 tokens");
            }
            if (expression.Count == 1)
            {
                if (!(expression[0] is Identifier) && !(expression[0] is IntLiteral) && !(expression[0] is BoolLiteral) && !(expression[0].Equals(new Keyword("this"))))
                {
                    addError(expression[0], "Expected identifier or value");
                }
                currentNode.token = realTokens[0];
                return;
            }
            if (expression[0] is Operator)
            {
                Operator op = (Operator)expression[0];
                if (op.value != "+" && op.value != "-" && op.value != "!")
                {
                    addError(expression[0], "Expected unary operator");
                    return;
                }
            }
            if (expression[expression.Count-1] is Operator)
            {
                addError(expression[expression.Count-1], "Did not expect operator");
                return;
            }
            int parenthesisDepth = 0;
            int parenthesisStart = -1;
            int arrayDepth = 0;
            int arrayStart = -1;
            int lastNew = -1;
            int lastSuperSpecial = -1; // [], () and ., eg: things[5], doThings(), thing.value
            int lastMultiplicative = -1;
            int lastAdditive = -1;
            int lastComparative = -1;
            int lastEquals = -1;
            int lastLogical = -1;
            if (expression[0].Equals(new Keyword("new")))
            {
                lastNew = 0;
            }
            if (expression[0].Equals(new Keyword("(")))
            {
                parenthesisStart = 0;
                parenthesisDepth++;
            }
            for (int i = 1; i < expression.Count-1; i++)
            {
                if (parenthesisDepth == 0 && arrayDepth == 0)
                {
                    if (expression[i].Equals(new Keyword(".")))
                    {
                        lastSuperSpecial = i;
                    }
                    if ((i == 1 || !expression[i-2].Equals(new Keyword("new"))) && (expression[i].Equals(new Keyword("[")) || expression[i].Equals(new Keyword("("))))
                    {
                        lastSuperSpecial = i;
                    }
                    if (expression[i] is Operator)
                    {
                        Operator op = (Operator)expression[i];
                        if (op.value == "*" || op.value == "/" || op.value == "%")
                        {
                            lastMultiplicative = i;
                        }
                        if (op.value == "+" || op.value == "-")
                        {
                            lastAdditive = i;
                        }
                        if (op.value == "<" || op.value == ">")
                        {
                            lastComparative = i;
                        }
                        if (op.value == "==")
                        {
                            lastEquals = i;
                        }
                        if (op.value == "&&" || op.value == "||")
                        {
                            lastLogical = i;
                        }
                    }
                }
                if (expression[i].Equals(new Keyword("(")))
                {
                    if (parenthesisDepth == 0)
                    {
                        parenthesisStart = i;
                    }
                    parenthesisDepth++;
                }
                if (expression[i].Equals(new Keyword(")")))
                {
                    if (parenthesisDepth == 0)
                    {
                        addError(expression[i], "Extra closing parenthesis");
                        return;
                    }
                    parenthesisDepth--;
                }
                if (expression[i].Equals(new Keyword("[")))
                {
                    if (arrayDepth == 0)
                    {
                        arrayStart = i;
                    }
                    arrayDepth++;
                }
                if (expression[i].Equals(new Keyword("]")))
                {
                    if (arrayDepth == 0)
                    {
                        addError(expression[arrayStart], "Extra array access closing brackets");
                    }
                    arrayDepth--;
                }
            }
            if (expression[expression.Count - 1].Equals(new Keyword("]")))
            {
                if (arrayDepth == 0)
                {
                    addError(expression[arrayStart], "Extra array access closing brackets");
                }
                arrayDepth--;
            }
            if (expression[expression.Count - 1].Equals(new Keyword(")")))
            {
                if (parenthesisDepth == 0)
                {
                    addError(expression[expression.Count - 1], "Extra closing parenthesis");
                    return;
                }
                parenthesisDepth--;
                if (parenthesisStart == 0)
                {
                    //whole expression is inside parenthesis
                    parseExprInternal(currentNode, expression.GetRange(1, expression.Count - 2), realTokens.GetRange(1, expression.Count - 2));
                    return;
                }
            }
            if (arrayDepth != 0)
            {
                addError(expression[arrayStart], "Unclosed array access brackets");
            }
            if (parenthesisDepth != 0)
            {
                addError(expression[parenthesisStart], "Unclosed parenthesis");
                return;
            }
            SyntaxNode left = new SyntaxNode(expression[0]);
            SyntaxNode right = new SyntaxNode(expression[0]);
            int splitAt = -1;
            if (lastLogical != -1)
            {
                splitAt = lastLogical;
                currentNode.token = expression[lastLogical];
            }
            else if (lastEquals != -1)
            {
                splitAt = lastEquals;
                currentNode.token = expression[lastEquals];
            }
            else if (lastComparative != -1)
            {
                splitAt = lastComparative;
                currentNode.token = expression[lastComparative];
            }
            else if (lastAdditive != -1)
            {
                splitAt = lastAdditive;
                currentNode.token = expression[lastAdditive];
            }
            else if (lastMultiplicative != -1)
            {
                splitAt = lastMultiplicative;
                currentNode.token = expression[lastMultiplicative];
            }
            else if (lastSuperSpecial != -1 && expression[lastSuperSpecial].Equals(new Keyword(".")))
            {
                splitAt = lastSuperSpecial;
                currentNode.token = expression[lastSuperSpecial];
            }
            else if (lastSuperSpecial != -1)
            {
                if (expression[lastSuperSpecial].Equals(new Keyword("[")))
                {
                    if (!expression[expression.Count-1].Equals(new Keyword("]")))
                    {
                        addError(expression[lastSuperSpecial], "Unclosed array access");
                        return;
                    }
                    parseExprInternal(right, expression.GetRange(lastSuperSpecial + 1, expression.Count - lastSuperSpecial - 2), realTokens.GetRange(lastSuperSpecial + 1, expression.Count - lastSuperSpecial - 2));
                    right.parent = currentNode;
                    currentNode.children.Add(right);
                }
                else if (expression[lastSuperSpecial].Equals(new Keyword("(")))
                {
                    if (!expression[expression.Count - 1].Equals(new Keyword(")")))
                    {
                        addError(expression[lastSuperSpecial], "Unclosed function call");
                        return;
                    }
                    List<SyntaxNode> arguments = parseArguments(expression.GetRange(lastSuperSpecial + 1, expression.Count - lastSuperSpecial - 2), realTokens.GetRange(lastSuperSpecial + 1, expression.Count - lastSuperSpecial - 2));
                    foreach (SyntaxNode n in arguments)
                    {
                        n.parent = currentNode;
                        currentNode.children.Add(n);
                    }
                }
                else
                {
                    throw new Exception("Unknown super special operator type");
                }
                parseExprInternal(left, expression.GetRange(0, lastSuperSpecial), realTokens.GetRange(0, lastSuperSpecial));
                left.parent = currentNode;
                currentNode.children.Insert(0, left);
                currentNode.token = expression[lastSuperSpecial];
                return;
            }
            else if (lastNew != -1)
            {
                currentNode.token = realTokens[lastNew];
                SyntaxNode child = new SyntaxNode(realTokens[lastNew + 1]);
                child.parent = currentNode;
                currentNode.children.Add(child);
                parseExprInternal(child, expression.GetRange(1, expression.Count - 1), realTokens.GetRange(1, realTokens.Count - 1));
                return;
            }
            if (splitAt != -1)
            {
                if (splitAt == 0 || splitAt == expression.Count-1)
                {
                    addError(expression[splitAt], "Binary expression must have two operands");
                    return;
                }
                parseExprInternal(left, expression.GetRange(0, splitAt), realTokens.GetRange(0, splitAt));
                parseExprInternal(right, expression.GetRange(splitAt + 1, expression.Count - splitAt - 1), realTokens.GetRange(splitAt + 1, expression.Count - splitAt - 1));
                left.parent = currentNode;
                right.parent = currentNode;
                currentNode.children.Add(left);
                currentNode.children.Add(right);
            }
            else
            {
                //unary ! - +
                currentNode.token = expression[0];
                SyntaxNode child = new SyntaxNode(expression[0]);
                parseExprInternal(child, expression.GetRange(1, expression.Count - 1), realTokens.GetRange(1, expression.Count - 1));
                currentNode.children.Add(child);
                child.parent = currentNode;
            }
        }
        private List<SyntaxNode> parseArguments(List<Token> expression, List<Token> realTokens)
        {

            StringBuilder sb = new StringBuilder();
            foreach (Token e in realTokens)
            {
                sb.Append(e).Append(", ");
            }
            System.Console.WriteLine("Parseargs " + sb.ToString());

            if (expression.Count == 0)
            {
                return new List<SyntaxNode>();
            }
            int start = 0;
            List<SyntaxNode> ret = new List<SyntaxNode>();
            int parenthesisDepth = 0;
            int parenthesisStart = 0;
            SyntaxNode parseNode;
            for (int i = 0; i < expression.Count; i++)
            {
                if (expression[i].Equals(new Keyword("(")))
                {
                    if (parenthesisDepth == 0)
                    {
                        parenthesisStart = i;
                    }
                    parenthesisDepth++;
                }
                if (expression[i].Equals(new Keyword(")")))
                {
                    if (parenthesisDepth == 0)
                    {
                        addError(expression[i], "Extra closing parenthesis");
                        break;
                    }
                    parenthesisDepth--;
                }
                if (parenthesisDepth == 0 && expression[i].Equals(new Keyword(",")))
                {
                    parseNode = new SyntaxNode(expression[0]);
                    parseExprInternal(parseNode, expression.GetRange(start, i - start), realTokens.GetRange(start, i - start));
                    ret.Add(parseNode);
                    i++;
                    start = i;
                }
            }
            parseNode = new SyntaxNode(expression[0]);
            parseExprInternal(parseNode, expression.GetRange(start, expression.Count - start), realTokens.GetRange(start, expression.Count - start));
            ret.Add(parseNode);
            if (parenthesisDepth != 0)
            {
                addError(expression[parenthesisStart], "Unclosed parenthesis");
            }
            return ret;
        }
        private void parse(SyntaxNode currentNode, List<Token> tokens, ref int loc)
        {
            if (currentNode.token.Equals(new NonTerminal("if statement")))
            {
                parseIf(currentNode, tokens, ref loc);
                return;
            }
            if (currentNode.token.Equals(new NonTerminal("expr")))
            {
                parseExpr(currentNode, tokens, ref loc);
                return;
            }
            if (languageTerminals.Contains(currentNode.token))
            {
                if (loc >= tokens.Count)
                {
                    addError(eof, eof, new List<Token>{currentNode.token});
                    throw new ParserEofReachedException();
                }
                if (!currentNode.token.compatible(tokens[loc]))
                {
                    addError(tokens[loc], tokens[loc], new List<Token> { currentNode.token });
                    while (!currentNode.token.compatible(tokens[loc]))
                    {
                        loc++;
                        if (loc >= tokens.Count)
                        {
                            addError(eof, eof, new List<Token> { currentNode.token });
                            throw new ParserEofReachedException();
                        }
                    }
                }
                currentNode.token = tokens[loc];
                currentNode.line = tokens[loc].line;
                currentNode.position = tokens[loc].position;
                loc++;
                return;
            }
            Token predictToken = getPredictToken(tokens, loc);
            if (!predict.ContainsKey(new Tuple<Token, Token>(currentNode.token, predictToken)))
            {
                List<Token> expectedSymbols = new List<Token>();
                foreach (Tuple<Token, Token> t in predict.Keys)
                {
                    if (t.Item1 == currentNode.token)
                    {
                        expectedSymbols.Add(t.Item2);
                    }
                }
                addError(predictToken, predictToken, expectedSymbols);
                while (!predict.ContainsKey(new Tuple<Token, Token>(currentNode.token, predictToken)))
                {
                    loc++;
                    predictToken = getPredictToken(tokens, loc);
                    if (predictToken == eof)
                    {
                        throw new ParserEofReachedException();
                    }
                }
            }
            List<Token> production = predict[new Tuple<Token, Token>(currentNode.token, predictToken)];
            foreach (Token t in production)
            {
                SyntaxNode child = new SyntaxNode(t);
                child.parent = currentNode;
                currentNode.children.Add(child);
                parse(child, tokens, ref loc);
            }
            if (currentNode.children.Count > 0)
            {
                currentNode.line = currentNode.children[0].line;
                currentNode.position = currentNode.children[0].position;
            }
        }
        public SyntaxTree parse(List<Token> tokens)
        {
            if (tokens.Count < 1)
            {
                throw new ParseError("Parser: Empty input");
            }
            errors = new List<String>();
            eofErroredOnce = false;
            eof.line = tokens.Last().line;
            eof.position = tokens.Last().position+1;
            if (startSymbol == null)
            {
                throw new ParserException("Parser: Start symbol is not set");
            }
            SyntaxNode root = new SyntaxNode(startSymbol);
            int loc = 0;
            try
            {
                parse(root, tokens, ref loc);
            }
            catch (ParserEofReachedException)
            {
            }
            if (errors.Count > 0)
            {
                throw new ParseError(errors);
            }
            return new SyntaxTree(root);
        }
    }
}
