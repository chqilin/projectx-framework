using System;
using System.Text;
using System.Collections;

namespace ProjectX
{
    public class Json
    {
        public static string Stringify(IList data)
        {
            if (data == null || data.Count == 0)
                return "[]";
            StringBuilder str = new StringBuilder("[");
            foreach (var node in data)
            {
                if (node is string)
                    str.AppendFormat("\"{0}\",", node);
                else if (node is IList)
                    str.AppendFormat("{0},", Stringify(node as IList));
                else if (node is XTable)
                    str.AppendFormat("{0},", Stringify(node as XTable));
                else
                    str.AppendFormat("{0},", node);
            }
            if (str[str.Length - 1] == ',')
            {
                str = str.Remove(str.Length - 1, 1);
            }
            str.Append("]");
            return str.ToString();
        }

        public static string Stringify(XTable data)
        {
            if (data == null || data.Count == 0)
                return "{}";
            StringBuilder str = new StringBuilder("{");
            foreach (var node in data.Nodes)
            {
                if (node.Value is string)
                    str.AppendFormat("\"{0}\":\"{1}\",", node.Key, node.Value);
                else if (node.Value is IList)
                    str.AppendFormat("\"{0}\":{1},", node.Key, Stringify(node.Value as IList));
                else if (node.Value is XTable)
                    str.AppendFormat("\"{0}\":{1},", node.Key, Stringify(node.Value as XTable));                
                else
                    str.AppendFormat("\"{0}\":{1},", node.Key, node.Value);
            }
            if (str[str.Length - 1] == ',')
            {
                str = str.Remove(str.Length - 1, 1);
            }
            str.Append("}");
            return str.ToString();
        }

        public static XTable Parse(string json)
        {
            JsonLexer lexer = new JsonLexer(json);
            object obj = lexer.NextValue();
            return obj as XTable;
        }

        sealed class JsonLexer
        {
            private string mSource = null;
            private int mPosition = 0;

            public JsonLexer(string src)
            {               
                this.mSource = src;
            }
            
            public object NextName()
            {
                char c = this.NextCleanChar();
                switch (c)
                {
                    case '\0': // eof
                        return null;

                    case '\'':
                    case '"':
                        return this.NextString(c);

                    default:
                        if(char.IsLower(c) || char.IsUpper(c) || c == '_')
                            return this.NextIdentifer(c);
                        else
                            return "";
                }
            }

            public object NextValue()
            {
                char c = NextCleanChar();
                switch (c)
                {
                    case '{':
                        return this.NextObject();

                    case '[':
                        return this.NextArray();

                    case '\'':
                    case '"':
                        return NextString(c);

                    default:
                        if(char.IsDigit(c) || c == '+' || c == '-')
                        {
                            return this.NextNumber(c);
                        }
                        else if (char.IsLower(c) || char.IsUpper(c) || c == '_')
                        {
                            int start = mPosition;
                            string name = this.NextIdentifer(c);
                            if (name == "true")
                                return true;
                            else if (name == "false")
                                return false;
                            else if (name == "null")
                                return null;
                            mPosition = start;
                        }
                        return null;
                }
            }

            XTable NextObject()
            {
                XTable result = new XTable();

                /* Peek to see if this is the empty object. */
                char first = this.NextCleanChar();
                if (first == '}')
                {
                    return result;
                }
                else if (first != '\0')
                {
                    mPosition--;
                }

                while (true)
                {
                    Object name = this.NextName();

                    /*
                     * Expect the name/value separator to be either a colon ':', an
                     * equals sign '=', or an arrow "=>". The last two are bogus but we
                     * include them because that's what the original implementation did.
                     */
                    char separator = this.NextCleanChar();
                    if (separator != ':' && separator != '=')
                    {
                        // error: "Expected ':' after " + name
                        return null;
                    }
                    if (mPosition < mSource.Length && mSource.Substring(mPosition, 1).ToCharArray()[0] == '>')
                    {
                        mPosition++;
                    }
                    result[(string)name] = this.NextValue();
                    
                    switch (this.NextCleanChar())
                    {
                        case '}':
                            return result;
                        case ';':
                        case ',':
                            continue;
                        default: // error: Unterminated object
                            return null;
                    }
                }
            }

            XTable NextArray()
            {
                XTable result = new XTable();
                int count = 0;

                char first = this.NextCleanChar();
                if (first == ']')
                {
                    return result;
                }
                else if (first != '\0')
                {
                    mPosition--;
                }

                while (true)
                {
                    object val = this.NextValue();
                    result[count++] = val;

                    switch (this.NextCleanChar())
                    {
                        case ']':
                            return result;
                        case ',':
                            continue;
                        default:  // error: Unterminated object
                            return null;
                    }
                }
            }

            /**
             * Returns the string up to but not including {@code quote}, unescaping any
             * character escape sequences encountered along the way. The opening quote
             * should have already been read. This consumes the closing quote, but does
             * not include it in the returned string.
             *
             * @param quote either ' or ".
             * @throws NumberFormatException if any unicode escape sequences are
             *     malformed.
             */
            string NextString(char quote)
            {
                StringBuilder builder = null;

                int start = mPosition;
                for(char c = this.NextChar(); c != '\0'; c = this.NextChar())
                {
                    if (c == quote)
                    {
                        if (builder == null)
                        {
                            // a new string avoids leaking memory
                            string str = mSource.Substring(start, mPosition - 1 - start);
                            return str;
                        }
                        else
                        {
                            builder.Append(mSource, start, mPosition - 1 - start);
                            return builder.ToString();
                        }
                    }

                    if (c == '\\')
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder();
                        }
                        builder.Append(mSource, start, mPosition - 1 - start);
                        builder.Append(this.EscapeChar());
                        start = mPosition;
                    }
                }

                // error : Unterminated string
                return "";
            }

            string NextNumber(char first)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(first);

                if (first != '+' && first != '-')
                {
                    char c = this.NextChar();
                    if(c == '.')
                    {
                        builder.Append(c);
                        c = this.NextChar();

                        while (char.IsDigit(c))
                        {
                            builder.Append(c);
                            c = this.NextChar();
                        }
                    }
                    else
                    {
                        while (char.IsDigit(c))
                        {
                            builder.Append(c);
                            c = this.NextChar();
                        }
                        if (c == '.')
                        {
                            builder.Append(c);
                            c = this.NextChar();
                        }
                        while (char.IsDigit(c))
                        {
                            builder.Append(c);
                            c = this.NextChar();
                        }
                    }
                }
                else
                {
                    char c = this.NextChar();
                    while (char.IsDigit(c))
                    {
                        builder.Append(c);
                        c = this.NextChar();
                    }
                    if (c == '.')
                    {
                        builder.Append(c);
                        c = this.NextChar();
                    }
                    while (char.IsDigit(c))
                    {
                        builder.Append(c);
                        c = this.NextChar();
                    }
                }

                mPosition--;

                return builder.ToString();
            }

            string NextIdentifer(char first)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(first);

                char c = this.NextChar();
                while(char.IsLower(c) || char.IsUpper(c) || char.IsDigit(c))
                {
                    builder.Append(c);
                    c = this.NextChar();
                }

                mPosition--;

                return builder.ToString();
            }

            char NextCleanChar()
            {
                for(char c = this.NextChar(); c != '\0'; c = this.NextChar())
                {
                    switch (c)
                    {
                        case ' ': case '\t': case '\n': case '\r':                     
                            continue;

                        case '/':
                            char next = this.NextChar();
                            switch (next)
                            {
                                case '*': /* c-style comment */
                                    int commentEnd = mSource.IndexOf("*/", mPosition);
                                    if (commentEnd == -1) // error
                                        return '\0';
                                    mPosition = commentEnd + 2;
                                    continue;

                                case '/': // cplusplus // line comment
                                    this.SkipToEndOfLine();
                                    continue;

                                default:
                                    return c;
                            }

                        case '#': // php # line comment
                            this.SkipToEndOfLine();
                            continue;

                        default:
                            return c;
                    }
                }

                return '\0';
            }

            /**
             * Advances the position until after the next newline character. If the line
             * is terminated by "\r\n", the '\n' must be consumed as whitespace by the
             * caller.
             */
            void SkipToEndOfLine()
            {
                char c = this.NextChar();
                while(c != '\0')
                {
                    if (c == '\r' || c == '\n')
                        break;
                    c = NextChar();
                }
            }

            char EscapeChar()
            {
                char c = this.NextChar();
                switch (c)
                {
                    case 'u':
                        if (mPosition + 4 > mSource.Length)
                        {
                            // error: Unterminated escape sequence
                            return ' ';
                        }
                        
                        string hex = mSource.Substring(mPosition, 4);
                        mPosition += 4;
                        return (char)Int16.Parse(hex);//Integer.parseInt(hex, 16);

                    case 'a': return '\a';
                    case 'b': return '\b';
                    case 'f': return '\f';
                    case 't': return '\t';
                    case 'n': return '\n';
                    case 'r': return '\r';
                    case 'v': return '\v';

                    case '\'':
                    case '"':
                    case '\\':
                    default:
                        return c;
                }
            }

            char NextChar()
            {
                if (mPosition >= mSource.Length)
                    return '\0';
                char c = this.mSource[mPosition];
                this.mPosition += 1;
                return c;
            }
        }
    }
}
