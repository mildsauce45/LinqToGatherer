using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToGatherer.Queryable.Contracts
{
    /// <summary>
    /// An interface to help smooth out the logic in the VisitBinary override method of UrlParameterBuilder
    /// </summary>
    internal interface INodeTypeVisitor<T>
    {
        /// <summary>
        /// Returns true if the expression was successfully visited
        /// </summary>
        bool Visit(Expression expression, IList<PropertyQueryInfo> urlParams);
    }
}
