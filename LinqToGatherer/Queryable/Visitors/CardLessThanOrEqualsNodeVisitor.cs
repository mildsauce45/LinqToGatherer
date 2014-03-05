using System.Collections.Generic;
using System.Linq.Expressions;
using LinqToGatherer.Queryable.Contracts;
using LinqToGatherer.Utility;

namespace LinqToGatherer.Queryable.Visitors
{
    [NodeType(ExpressionType.LessThanOrEqual)]
    internal class CardLessThanOrEqualsNodeVisitor : INodeTypeVisitor<Card>
    {
        public bool Visit(Expression node, IList<PropertyQueryInfo> urlParams)
        {
            var handled = false;

            if (node.IsMemberValueExpression<Card>("CMC"))
            {
                urlParams.Add(new PropertyQueryInfo("cmc", node.GetValueFromExpression<Card, int>("CMC").ToString(), Constants.NumericUrlOperations.L_OR_E));
                handled = true;
            }
            else if (node.IsMemberValueExpression<Card>("Power"))
            {
                urlParams.Add(new PropertyQueryInfo("power", node.GetValueFromExpression<Card, int>("Power").ToString(), Constants.NumericUrlOperations.L_OR_E));
                handled = true;
            }
            else if (node.IsMemberValueExpression<Card>("Toughness"))
            {
                urlParams.Add(new PropertyQueryInfo("tough", node.GetValueFromExpression<Card, int>("Toughness").ToString(), Constants.NumericUrlOperations.L_OR_E));
                handled = true;
            }

            return handled;
        }
    }
}
