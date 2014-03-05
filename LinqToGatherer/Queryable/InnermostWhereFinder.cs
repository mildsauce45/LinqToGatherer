using System.Linq.Expressions;

namespace LinqToGatherer
{
    /// <summary>
    /// An expression visitor designed to locate the actual filtering within a LINQ expression
    /// </summary>
    internal class InnermostWhereFinder : ExpressionVisitor
    {
        private MethodCallExpression innermostWhereExpression;

        public MethodCallExpression GetInnermostWhere(Expression expression)
        {
            Visit(expression);

            return innermostWhereExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "Where")
                innermostWhereExpression = node;

            Visit(node.Arguments[0]);

            return node;
        }
    }
}
