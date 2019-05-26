using System;
using System.Linq;

namespace BooleanRetrieval.Logic.QueryParsing.Tokenize
{
    public class TokenReader
    {
        private string _query;
        private int _position;
        private char _currentChar = char.MaxValue;
        private string _currentTerm;
        private Token _currentToken = Token.EOL;
        private Token _prevToken = Token.EOL;

        private const char AND = '&';
        private const char OR = '|';
        private const char NOT = '!';
        private const char OPEN_BRACKET = '(';
        private const char CLOSE_BRACKET = ')';

        public TokenReader(string query)
        {
            _query = query;
        }

        public Token Token => _currentToken;

        public Token PrevToken => _prevToken;

        public string Term => _currentTerm;

        public void NextToken()
        {
            _prevToken = _currentToken;

            if (_currentChar == char.MaxValue)
            {
                NextChar();
            }

            SkipWhitespaces();

            switch (_currentChar)
            {
                case char.MinValue:
                    _currentToken = Token.EOL;
                    return;

                case AND:
                    NextChar();
                    if (_currentChar == AND)
                    {
                        NextChar();
                    }

                    _currentToken = Token.And;
                    return;

                case OR:
                    NextChar();
                    if (_currentChar == OR)
                    {
                        NextChar();
                    }

                    _currentToken = Token.Or;
                    return;

                case NOT:
                    // We consider space(s) between term and NOT as AND operation.
                    if (_prevToken == Token.Term)
                    {
                        _currentToken = Token.And;
                        return;
                    }

                    NextChar();
                    _currentToken = Token.Not;
                    return;

                case OPEN_BRACKET:
                    NextChar();
                    _currentToken = Token.OpenBracket;
                    return;

                case CLOSE_BRACKET:
                    NextChar();
                    _currentToken = Token.CloseBracket;
                    return;
            }

            if (char.IsLetterOrDigit(_currentChar) || SearchOption.AcceptableSymbols.Contains(_currentChar))
            {
                // We consider space(s) between terms as AND operation.
                if (_prevToken == Token.Term)
                {
                    _currentToken = Token.And;

                    return;
                }

                var i = _position - 1;

                while ((char.IsLetterOrDigit(_currentChar) || SearchOption.AcceptableSymbols.Contains(_currentChar))
                    && _currentChar != char.MinValue)
                {
                    NextChar();
                }

                _currentTerm = _query.Substring(i, _position - i - (_currentChar == char.MinValue ? 0 : 1)).ToLower();
                _currentToken = Token.Term;

                return;
            }

            throw new Exception($"Unexpected character: {_currentChar}");
        }

        private void NextChar()
        {
            if (_position >= _query.Length)
            {
                _currentChar = char.MinValue;
                return;
            }

            _currentChar = _query[_position++];
        }

        private void SkipWhitespaces()
        {
            while (char.IsWhiteSpace(_currentChar))
            {
                NextChar();
            }
        }
    }
}
