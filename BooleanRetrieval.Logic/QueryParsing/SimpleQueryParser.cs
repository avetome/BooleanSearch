using System;
using System.Collections.Generic;

using BooleanRetrieval.Logic.QueryParsing.Tokenize;

namespace BooleanRetrieval.Logic.QueryParsing
{
    /// <summary>
    /// Very simple query parser.
    /// Return just a list with terms and operands together.
    /// Don't understand brackets.
    /// </summary>
    public class SimpleQueryParser
    {
        private Tokenizer _tokenizer;

        public SimpleQueryParser(Tokenizer tokenizer)
        {
            _tokenizer = tokenizer;
        }

        public SimpleQueryParser(string str) : this(new Tokenizer(str))
        {
        }

        public string[] SimpleParse()
        {
            var result = new List<string>();

            bool waitingTermOrNot = true;
            bool waitionOperation = false;
            while (true)
            {
                if (_tokenizer.Token == Token.EOL)
                {
                    if (waitingTermOrNot)
                    {
                        throw new Exception("Invalid search string format");
                    }

                    break;
                }

                if (waitingTermOrNot)
                {
                    if (_tokenizer.Token == Token.Term)
                    {
                        result.Add(_tokenizer.Term);

                        waitingTermOrNot = false;
                        waitionOperation = true;
                    }
                    else if (_tokenizer.Token == Token.Not)
                    {
                        result.Add("NOT");
                    }
                    else
                    {
                        throw new Exception("Invalid search string format");
                    }
                }
                else if (waitionOperation)
                {
                    if (_tokenizer.Token == Token.And)
                    {
                        result.Add("AND");
                    }
                    else if (_tokenizer.Token == Token.Or)
                    {
                        result.Add("OR");
                    }
                    else
                    {
                        throw new Exception("Invalid search string format");
                    }

                    waitingTermOrNot = true;
                    waitionOperation = false;
                }

                _tokenizer.NextToken();
            }

            return result.ToArray();
        }
    }
}