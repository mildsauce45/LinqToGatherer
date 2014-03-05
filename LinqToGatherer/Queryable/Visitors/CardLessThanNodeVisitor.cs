using System.Collections.Generic;
using System.Linq.Expressions;
using LinqToGatherer.Queryable.Contracts;
using LinqToGatherer.Utility;

namespace LinqToGatherer.Queryable.Visitors
{
    [NodeType(ExpressionType.LessThan)]
    internal class CardLessThanNodeVisitor : INodeTypeVisitor<Card>
    {
        public bool Visit(Expression node, IList<PropertyQueryInfo> urlParams)
        {
            var handled = false;

            if (node.IsMemberValueExpression<Card>("CMC"))
            {
                urlParams.Add(new PropertyQueryInfo("cmc", node.GetValueFromExpression<Card, int>("CMC").ToString(), Constants.NumericUrlOperations.LESS_THAN));
                handled = true;
            }
            else if (node.IsMemberValueExpression<Card>("Power"))
            {
                urlParams.Add(new PropertyQueryInfo("power", node.GetValueFromExpression<Card, int>("Power").ToString(), Constants.NumericUrlOperations.LESS_THAN));
                handled = true;
            }
            else if (node.IsMemberValueExpression<Card>("Toughness"))
            {
                urlParams.Add(new PropertyQueryInfo("tough", node.GetValueFromExpression<Card, int>("Toughness").ToString(), Constants.NumericUrlOperations.LESS_THAN));
                handled = true;
            }

            return handled;
        }
    }
}
