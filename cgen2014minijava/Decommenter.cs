using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cgen2014minijava
{
    public class Decommenter
    {
        public Decommenter()
        {

        }
        public String decomment(String input)
        {
            StringBuilder ret = new StringBuilder();
            int start = 0;
            int commentDepth = 0;
            bool endOfLineComment = false;
            bool insideString = false;
            if (input[0] == '"')
            {
                insideString = true;
            }
            for (int i = 1; i < input.Length; i++)
            {
                if (commentDepth == 0 && !endOfLineComment && input[i] == '"' && input[i-1] != '\\')
                {
                    insideString = !insideString;
                }
                if (!insideString)
                {
                    if (input[i] == '*' && input[i - 1] == '/')
                    {
                        if (commentDepth == 0 && !endOfLineComment)
                        {
                            ret.Append(input.Substring(start, i - start - 1));
                        }
                        commentDepth++;
                        i++;
                    }
                    else if (input[i] == '/' && input[i - 1] == '*')
                    {
                        commentDepth--;
                        if (commentDepth == 0 && !endOfLineComment)
                        {
                            start = i + 1;
                        }
                        i++;
                    }
                    else if (input[i] == '/' && input[i - 1] == '/')
                    {
                        if (commentDepth == 0 && !endOfLineComment)
                        {
                            ret.Append(input.Substring(start, i - start - 1));
                        }
                        endOfLineComment = true;
                        i++;
                    }
                    else if (input[i] == '\n')
                    {
                        if (commentDepth == 0 && endOfLineComment)
                        {
                            start = i + 1;
                        }
                        endOfLineComment = false;
                    }
                }
            }
            ret.Append(input.Substring(start));
            return ret.ToString();
        }
    }
}
