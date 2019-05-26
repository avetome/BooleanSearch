using BooleanRetrieval.Logic.QueryParsing.Tokenize;
using System;
using System.Collections.Generic;
using System.Text;

namespace BooleanRetrieval.Logic.QueryParsing
{
    public class EtQueryParser
    {
        private TokenReader _tokenReader;

        public ExpressionTreeNode Parse(string query)
        {
            _tokenReader = new TokenReader(query);

            _tokenReader.NextToken();

            var result = ParseAnd();

            if (_tokenReader.Token != Token.EOL)
            {
                throw new Exception("Invalid search string format");
            }

            return result;
        }

        private ExpressionTreeNode ParseAnd()
        {
            var node = ParseOr();

            if (_tokenReader.Token != Token.And)
            {
                return node;
            }

            _tokenReader.NextToken();

            ExpressionTreeNode root = ExpressionTreeNode.CreateAnd(node, ParseOr());
            node = root;

            while (true)
            {
                if (_tokenReader.Token != Token.And)
                {
                    return root;
                }

                _tokenReader.NextToken();

                var child1 = node.Child2;

                node.Child2 = ExpressionTreeNode.CreateAnd(child1, ParseOr());

                node = node.Child2;
            }
        }

        private ExpressionTreeNode ParseOr()
        {
            var node = ParseNot();

            _tokenReader.NextToken();

            if (_tokenReader.Token != Token.Or)
            {
                return node;
            }

            _tokenReader.NextToken();

            ExpressionTreeNode root = ExpressionTreeNode.CreateOr(node, ParseNot());
            node = root;

            _tokenReader.NextToken();

            while (true)
            {
                if (_tokenReader.Token != Token.Or)
                {
                    return root;
                }

                _tokenReader.NextToken();

                var child1 = node.Child2;

                node.Child2 = ExpressionTreeNode.CreateOr(child1, ParseNot());

                node = node.Child2;

                _tokenReader.NextToken();
            }
        }

        private ExpressionTreeNode ParseNot()
        {
            if (_tokenReader.Token != Token.Not)
            {
                return ParseTerm();
            }

            _tokenReader.NextToken();

            var term = ParseTerm();

            return ExpressionTreeNode.CreateNot(term);
        }

        private ExpressionTreeNode ParseTerm()
        {
            if (_tokenReader.Token != Token.Term)
            {
                throw new Exception("Invalid search string format");
            }

            return ExpressionTreeNode.CreateTerm(_tokenReader.Term);
        }
    }
}
