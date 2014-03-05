using System.Collections.Generic;
using System.Linq.Expressions;
using LinqToGatherer.Queryable.Contracts;
using LinqToGatherer.Utility;

namespace LinqToGatherer.Queryable.Visitors
{
    [NodeType(ExpressionType.Equal)]
    internal class CardImageEqualsNodeVisitor : INodeTypeVisitor<CardImage>
    {
        public bool Visit(Expression expression, IList<PropertyQueryInfo> urlParams)
        {
            // We can only query against the multivserse id for the images. 
            if (expression.IsMemberValueExpression<CardImage>("MultiverseId"))
            {
                urlParams.Add(new PropertyQueryInfo("multiverseid", expression.GetValueFromExpression<CardImage, int>("MultiverseId").ToString()));
                return true;
            }

            return false;
        }
    }
}
