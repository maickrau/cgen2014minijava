using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace comp2014minipl
{
    public class RegexException : Exception
    {
        public RegexException(String str) : base(str) { }
    }
    public class Regex
    {
        public NFA nfa;
        public Regex(String str)
        {
            int loc = 0;
            nfa = parseRegex(str, ref loc);
        }
        private NFA parseRegex(String str, ref int loc)
        {
            NFA builtNFA = new NFA("");
            while (loc < str.Length)
            {
                NFA nextNFA = parseOne(str, ref loc);
                builtNFA = builtNFA.conc(nextNFA);
            }
            return builtNFA;
        }
        public int recognize(String str)
        {
            int longestRecognition = 0;
            nfa.startRecognizing();
            for (int i = 0; i < str.Length; i++)
            {
                nfa.takeCharacter(str[i]);
                int recognizes = nfa.recognizesCurrent();
                if (recognizes > -1)
                {
                    longestRecognition = recognizes;
                }
            }
            return longestRecognition;
        }
        public List<Tuple<int, int>> recognizeAll(String str)
        {
            List<Tuple<int, int>> all = new List<Tuple<int, int>>();
            nfa.startRecognizing();
            for (int i = 0; i < str.Length; i++)
            {
                nfa.takeCharacter(str[i]);
                nfa.alsoRecognizeFromHere();
                int recognizes = nfa.recognizesCurrent();
                if (recognizes > -1)
                {
                    all.Add(new Tuple<int, int>(i + 1 - recognizes, i + 1));
                }
            }
            return all;
        }
        private NFA parseParenthesis(string str, ref int loc)
        {
            //str[loc] == '('
            loc++;
            NFA totalNFA = null;
            NFA builtNFA = null;
            while (str[loc] != ')')
            {
                if (loc >= str.Length)
                {
                    throw new RegexException("Regex has a ( without a matching )");
                }
                if (str[loc] == '|')
                {
                    if (totalNFA == null)
                    {
                        totalNFA = builtNFA;
                    }
                    else
                    {
                        totalNFA = totalNFA.or(builtNFA);
                    }
                    builtNFA = null;
                    loc++;
                }
                NFA nextNFA = parseOne(str, ref loc);
                if (builtNFA == null)
                {
                    builtNFA = nextNFA;
                }
                else
                {
                    builtNFA = builtNFA.conc(nextNFA);
                }
            }
            if (totalNFA == null)
            {
                totalNFA = builtNFA;
            }
            else
            {
                totalNFA = totalNFA.or(builtNFA);
            }
            loc++;
            //loc is first character after )
            return totalNFA;
        }
        private NFA parseOne(String str, ref int loc)
        {
            NFA retNFA;
            if (str[loc] == '(')
            {
                retNFA = parseParenthesis(str, ref loc);
            }
            else if (str[loc] == '[')
            {
                retNFA = parseAlternates(str, ref loc);
            }
            else if (str[loc] == '\\')
            {
                retNFA = parseSpecial(str, ref loc);
            }
            else if (str[loc] == '.')
            {
                retNFA = new NFA(1);
                loc++;
            }
            else
            {
                retNFA = new NFA(str.Substring(loc, 1));
                loc++;
            }
            if (loc < str.Length)
            {
                if (str[loc] == '*')
                {
                    retNFA = retNFA.closure();
                    loc++;
                }
                else if (str[loc] == '?')
                {
                    retNFA = retNFA.maybe();
                    loc++;
                }
            }
            return retNFA;
        }
        private NFA parseAlternates(String str, ref int loc)
        {
            //str[loc] == '['
            loc++;
            bool exclusion = false;
            if (str[loc] == '^')
            {
                exclusion = true;
                loc++;
            }
            NFA builtNFA = null;
            while (str[loc] != ']')
            {
                if (loc >= str.Length)
                {
                    throw new RegexException("Regex has a [ without a matching ]");
                }
                NFA nextNFA;
                if (str[loc] == '\\')
                {
                    nextNFA = parseSpecial(str, ref loc);
                }
                else if (loc < str.Length-1 && str[loc+1] == '-')
                {
                    char start = str[loc];
                    loc += 2;
                    char end = str[loc];
                    HashSet<char> chars = new HashSet<char>();
                    for (char ch = start; ch <= end; ch++)
                    {
                        chars.Add(ch);
                    }
                    nextNFA = new NFA(chars);
                    loc++;
                }
                else
                {
                    nextNFA = new NFA(str.Substring(loc, 1));
                    loc++;
                }
                if (builtNFA == null)
                {
                    builtNFA = nextNFA;
                }
                else
                {
                    builtNFA = builtNFA.or(nextNFA);
                }
            }
            if (exclusion)
            {
                builtNFA = builtNFA.complement();
            }
            builtNFA = builtNFA.complement().or(new NFA(1).complement()).complement();
            loc++;
            //loc is first character after ]
            return builtNFA;
        }
        private NFA parseSpecial(String str, ref int loc)
        {
            //str[loc] == '\\'
            loc++;
            if (loc >= str.Length)
            {
                throw new RegexException("Regex ends with a special character marker");
            }
            NFA ret;
            switch(str[loc])
            {
                case '|':
                    ret = new NFA("|");
                    break;
                case '\\':
                    ret = new NFA("\\");
                    break;
                case 'n':
                    ret = new NFA("\n");
                    break;
                case 't':
                    ret = new NFA("\t");
                    break;
                case 'r':
                    ret = new NFA("\r");
                    break;
                case '\'':
                    ret = new NFA("\'");
                    break;
                case '\"':
                    ret = new NFA("\"");
                    break;
                case '*':
                    ret = new NFA("*");
                    break;
                case '(':
                    ret = new NFA("(");
                    break;
                case ')':
                    ret = new NFA(")");
                    break;
                case '[':
                    ret = new NFA("[");
                    break;
                case ']':
                    ret = new NFA("]");
                    break;
                case '.':
                    ret = new NFA(".");
                    break;
                case '?':
                    ret = new NFA("?");
                    break;
                case '^':
                    ret = new NFA("^");
                    break;
                case '-':
                    ret = new NFA("-");
                    break;
                default:
                    throw new RegexException("Unknown escape character in regex");
            }
            loc++;
            //loc is first character after special
            return ret;
        }
    }
}
