using System;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToGatherer
{
    /// <summary>
    /// Implementation of IQueryProvider. Handles the creation and exeuction of queries, though the grunt work of execution is handed off
    /// to GathererQueryContext
    /// </summary>
    internal class GathererQueryProvider<TData> : IQueryProvider
    {
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new GathererQueryable<TElement>(this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type;
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(GathererQueryable<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch
            {
                return null;
            }
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var canEnumerate = typeof(TResult).Name == "IEnumerable`1";

            return (TResult)GathererQueryContext.Execute<TData>(expression, canEnumerate);
        }

        public object Execute(Expression expression)
        {
            return GathererQueryContext.Execute<TData>(expression, false);
        }        
    }
}
