using System.Diagnostics;

namespace BooleanRetrieval.Logic.QueryParsing
{
    [DebuggerDisplay("{string.IsNullOrEmpty(Term) ? Operation : Term}")]
    public class ExpressionTreeNode
    {
        public string Operation { get; set; }

        public string Term { get; set; }

        public ExpressionTreeNode Child1 { get; set; }

        public ExpressionTreeNode Child2 { get; set; }

        public static ExpressionTreeNode CreateTerm(string term)
        {
            return new ExpressionTreeNode() { Term = term };
        }

        public static ExpressionTreeNode CreateNot(ExpressionTreeNode term)
        {
            return new ExpressionTreeNode() { Operation = "NOT", Child1 = term };
        }

        public static ExpressionTreeNode CreateOr(ExpressionTreeNode child1, ExpressionTreeNode child2)
        {
            return new ExpressionTreeNode() { Operation = "OR", Child1 = child1, Child2 = child2 };
        }

        public static ExpressionTreeNode CreateAnd(ExpressionTreeNode child1, ExpressionTreeNode child2)
        {
            return new ExpressionTreeNode() { Operation = "AND", Child1 = child1, Child2 = child2 };
        }

        public static ExpressionTreeNode CreateAllNode()
        {
            return new ExpressionTreeNode() { Operation = "ALL" };
        }

        public static ExpressionTreeNode CreateZeroNode()
        {
            return new ExpressionTreeNode() { Operation = "ZERO" };
        }
    }
}
