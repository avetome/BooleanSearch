using System;
using System.Collections.Generic;
using System.Text;

namespace BooleanRetrieval.Logic.QueryParsing.Tokenize
{
    public enum Token
    {
        Term,
        And,
        Or,
        Not,
        OpenBracket,
        CloseBracket,
        EOL
    }
}
