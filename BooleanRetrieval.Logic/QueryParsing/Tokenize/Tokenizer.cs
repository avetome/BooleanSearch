using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BooleanRetrieval.Logic.QueryParsing.Tokenize
{
    public class Tokenizer
    {
        private string _query;
        private int _position;
        private char _currentChar;
        private string _currentTerm;
        private Token _currentToken;
        private Token _prevToken;

        public Tokenizer(string query)
        {
            _query = query;
            _position = 0;

            _currentToken = Token.None;

            NextChar();
            NextToken();
        }

        public Token Token => _currentToken;

        public Token PrevToken => _prevToken;

        public string Term => _currentTerm;

        private void NextChar()
        {
            if (_position >= _query.Length)
            {
                _currentChar = '\0';
                return;
            }

            _currentChar = _query[_position++];
        }

        public void NextToken()
        {
            _prevToken = _currentToken;

            SkipWhitespaces();

            switch (_currentChar)
            {
                case '\0':
                    _currentToken = Token.EOL;
                    return;

                case '&':
                    NextChar();
                    if (_currentChar == '&')
                    {
                        NextChar();
                    }

                    _currentToken = Token.And;
                    return;

                case '|':
                    NextChar();
                    if (_currentChar == '|')
                    {
                        NextChar();
                    }

                    _currentToken = Token.Or;
                    return;

                case '!':
                    // We consider space(s) between term and NOT as AND operation.
                    if (_prevToken == Token.Term)
                    {
                        _currentToken = Token.And;
                        return;
                    }

                    NextChar();
                    _currentToken = Token.Not;
                    return;

                case '(':
                    NextChar();
                    _currentToken = Token.OpenBracket;
                    return;

                case ')':
                    NextChar();
                    _currentToken = Token.CloseBracket;
                    return;
            }

            if (char.IsLetterOrDigit(_currentChar)
                || _currentChar == '-'
                || _currentChar == '.'
                || _currentChar == '/')
            {
                // We consider space(s) between terms as AND operation.
                if (_prevToken == Token.Term)
                {
                    _currentToken = Token.And;

                    return;
                }

                var i = _position - 1;

                while ((char.IsLetterOrDigit(_currentChar)
                    || _currentChar == '-'
                    || _currentChar == '.'
                    || _currentChar == '/')
                    && _currentChar != '\0')
                {
                    NextChar();
                }

                _currentTerm = _query.Substring(i, _position - i - (_position == _query.Length ? 0 : 1)).ToLower();
                _currentToken = Token.Term;

                return;
            }

            throw new InvalidDataException($"Unexpected character: {_currentChar}");
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
