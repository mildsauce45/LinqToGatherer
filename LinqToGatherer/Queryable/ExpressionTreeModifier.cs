using System.Linq;
using System.Linq.Expressions;

namespace LinqToGatherer
{
    internal class ExpressionTreeModifier<T> : ExpressionVisitor
    {
        private IQueryable<T> queryable;

        internal ExpressionTreeModifier(IQueryable<T> queryable)
        {
            this.queryable = queryable;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return node.Type == typeof(GathererQueryable<T>) ? Expression.Constant(queryable) : node;
        }
    }
}
