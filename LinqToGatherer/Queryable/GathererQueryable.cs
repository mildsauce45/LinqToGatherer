using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToGatherer
{
    /// <summary>
    /// The implementation of IQueryable for the purposes of this project. We actually implement IOrderedQueryable in order to support ordering operations
    /// </summary>
    internal class GathererQueryable<TData> : IOrderedQueryable<TData>
    {
        #region Properties

        public IQueryProvider Provider { get; private set; }
        public Expression Expression { get; private set; }

        public Type ElementType
        {
            get { return typeof(TData); }
        }

        #endregion

        #region Constructors

        public GathererQueryable()
        {
            Provider = new GathererQueryProvider<TData>();
            Expression = Expression.Constant(this);
        }

        internal GathererQueryable(IQueryProvider provider, Expression expression)
        {
            if (provider == null)
                provider = new GathererQueryProvider<TData>();

            if (expression == null)
                Expression = Expression.Constant(this);

            if (!typeof(IQueryable<TData>).IsAssignableFrom(expression.Type))
                throw new InvalidOperationException("expression");

            Provider = provider;
            Expression = expression;
        }

        #endregion

        #region IEnumerable<T> Implementation

        public IEnumerator<TData> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<TData>>(Expression).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Provider.Execute<System.Collections.IEnumerable>(Expression).GetEnumerator();
        }

        #endregion
    }
}
