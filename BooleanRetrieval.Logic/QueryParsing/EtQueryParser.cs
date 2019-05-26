using BooleanRetrieval.Logic.QueryParsing.Tokenize;
using System;
using System.Collections.Generic;
using System.Text;

namespace BooleanRetrieval.Logic.QueryParsing
{
    public class EtQueryParser
    {
        private TokenReader _tokenReader;

        /// <summary>
        /// Functions to optimize expression query for faster searcher.
        /// Maybe should be separated, but let's stay here for now.
        /// </summary>
        private List<Func<ExpressionTreeNode, ExpressionTreeNode>> _optimizeActions =
            new List<Func<ExpressionTreeNode, ExpressionTreeNode>>() { TwoNotInAndRule };

        public ExpressionTreeNode Parse(string query)
        {
            _tokenReader = new TokenReader(query);

            _tokenReader.NextToken();

            var result = ParseAnd();

            if (_tokenReader.Token != Token.EOL)
            {
                throw new Exception("Invalid search string format");
            }

            result = OptimizeTree(result);

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

        private ExpressionTreeNode OptimizeTree(ExpressionTreeNode node)
        {
            if (node == null)
            {
                return null;
            }

            foreach(var action in _optimizeActions)
            {
                node = action(node);
            }

            node.Child1 = OptimizeTree(node.Child1);
            node.Child2 = OptimizeTree(node.Child2);

            return node;
        }

        /// <summary>
        /// AND(NOT,NOT) -> NOT(OR)
        /// </summary>
        /// <param name="node"></param>
        private static ExpressionTreeNode TwoNotInAndRule(ExpressionTreeNode node)
        {
            if (node == null)
            {
                return null;
            }

            if (node.Operation == "AND" && node.Child1.Operation == "NOT" && node.Child2.Operation == "NOT")
            {
                var andChild = ExpressionTreeNode.CreateOr(node.Child1.Child1, node.Child2.Child1);

                node = ExpressionTreeNode.CreateNot(andChild);
            }

            return node;
        }
    }
}
