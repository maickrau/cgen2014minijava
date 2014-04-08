﻿using System;
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
                        currentNode.token = realTokens[0];
                        SyntaxNode id = new SyntaxNode(realTokens[1]);
                        id.parent = currentNode;
                        currentNode.children.Add(id);
                        return;
                    }
                }
            }
            parseExprInternal(currentNode, thisExpression, realTokens);
        }
        private void parseExprInternal(SyntaxNode currentNode, List<Token> expression, List<Token> realTokens)
        {
            /*
            StringBuilder sb = new StringBuilder();
            foreach (Token e in realTokens)
            {
                sb.Append(e).Append(", ");
            }
            System.Console.WriteLine("Parseexpr " + sb.ToString());
             */
            if (expression.Count == 0)
            {
                throw new Exception("Something went wrong! Expression has 0 tokens");
            }
            if (expression.Count == 1)
            {
                if (!(expression[0] is Identifier) && !(expression[0] is IntLiteral) && !(expression[0] is BoolLiteral))
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
            int firstMultiplicative = -1;
            int firstAdditive = -1;
            int firstComparative = -1;
            int firstEquals = -1;
            int firstLogical = -1;
            if (expression[0].Equals(new Keyword("(")))
            {
                parenthesisStart = 0;
                parenthesisDepth++;
            }
            for (int i = 1; i < expression.Count-1; i++)
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
                        return;
                    }
                    parenthesisDepth--;
                }
                if (parenthesisDepth == 0 && expression[i] is Operator)
                {
                    Operator op = (Operator)expression[i];
                    if ((op.value == "*" || op.value == "/" || op.value == "%") && firstMultiplicative == -1)
                    {
                        firstMultiplicative = i;
                    }
                    if ((op.value == "+" || op.value == "-")  && firstAdditive == -1)
                    {
                        firstAdditive = i;
                    }
                    if ((op.value == "<" || op.value == ">") && firstComparative == -1)
                    {
                        firstComparative = i;
                    }
                    if ((op.value == "==") && firstEquals == -1)
                    {
                        firstEquals = i;
                    }
                    if ((op.value == "&&" || op.value == "||") && firstLogical == -1)
                    {
                        firstLogical = i;
                        break;
                    }
                }
            }
            if (expression[expression.Count-1].Equals(new Keyword(")")))
            {
                if (parenthesisDepth == 0)
                {
                    addError(expression[expression.Count - 1], "Extra closing parenthesis");
                    return;
                }
                parenthesisDepth--;
            }
            if (parenthesisDepth != 0)
            {
                addError(expression[parenthesisStart], "Unclosed parenthesis");
                return;
            }
            SyntaxNode left = new SyntaxNode(expression[0]);
            SyntaxNode right = new SyntaxNode(expression[0]);
            int splitAt = -1;
            if (firstLogical != -1)
            {
                splitAt = firstLogical;
                currentNode.token = expression[firstLogical];
            }
            else if (firstEquals != -1)
            {
                splitAt = firstEquals;
                currentNode.token = expression[firstEquals];
            }
            else if (firstComparative != -1)
            {
                splitAt = firstComparative;
                currentNode.token = expression[firstComparative];
            }
            else if (firstAdditive != -1)
            {
                splitAt = firstAdditive;
                currentNode.token = expression[firstAdditive];
            }
            else if (firstMultiplicative != -1)
            {
                splitAt = firstMultiplicative;
                currentNode.token = expression[firstMultiplicative];
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
                if (expression[0].Equals(new Keyword("(")))
                {
                    //whole expression is inside parenthesis
                    parseExprInternal(currentNode, expression.GetRange(1, expression.Count - 2), realTokens.GetRange(1, expression.Count - 2));
                    return;
                }
                //unary ! - +
                currentNode.token = expression[0];
                SyntaxNode child = new SyntaxNode(expression[0]);
                parseExprInternal(child, expression.GetRange(1, expression.Count - 1), realTokens.GetRange(1, expression.Count - 1));
                currentNode.children.Add(child);
                child.parent = currentNode;
            }
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
