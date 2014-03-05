using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqToGatherer.Utility;

namespace LinqToGatherer
{
    internal class GathererQueryContext
    {
        private static readonly Type setsType = typeof(string);

        internal static object Execute<T>(Expression expression, bool canEnumerate)
        {
            // The expression must represent a query over the data source.
            if (!IsQueryOverDataSource(expression))
                throw new InvalidProgramException("Invalid query");

            var urlParamBuilder = new UrlParameterBuilder<T>();

            var whereFinder = new InnermostWhereFinder();
            var whereExpression = whereFinder.GetInnermostWhere(expression);

            if (whereExpression != null)
            {
                var lambda = (LambdaExpression)((UnaryExpression)(whereExpression.Arguments[1])).Operand;

                // We need to evaluate variable names to set the real values into the query.
                lambda = (LambdaExpression)Evaluator.PartialEval(lambda);

                urlParamBuilder.Visit(lambda.Body);
            }
            else
            {
                if (typeof(T) != setsType)
                    throw new Exception("Only the Sets query allows querying without specifying a where clause.");
            }

            // When querying for the sets, all we're doing is fetching the list from the drop down on the front page.
            // There's no need to complicated prepocessing of the arguments since .NET can handle the full list with ease
            if (urlParamBuilder.Parameters.Count == 0 && typeof(T) != setsType)
                throw new Exception("What exactly are we querying for now?");

            // We need to figure out, based on the type what call to gatherer we need to make
            var gathererCall = GetGathererCall(typeof(T));

            // If we cant figure it out, return the empty set
            if (gathererCall == null)
                return Enumerable.Empty<T>().AsQueryable().Provider.Execute(expression);
            
            // Call out to the GathererConnection method and return the results
            var results = gathererCall.Invoke(null, new[] { urlParamBuilder.Parameters }) as IEnumerable<T>;

            // Copy the result to an IQueryable
            var queryable = results.AsQueryable();

            // Copy the expression tree that was passed in, changing only the first argument of the innermost MethodCallExpression.
            var treeCopier = new ExpressionTreeModifier<T>(queryable);
            var newExpressionTree = treeCopier.Visit(expression);

            // This step creates an IQueryable the executes by replacing queryable method with enumerable method.
            return canEnumerate ? queryable.Provider.CreateQuery(newExpressionTree) : queryable.Provider.Execute(newExpressionTree);
        }

        private static bool IsQueryOverDataSource(Expression expression)
        {
            return expression is MethodCallExpression;
        }

        private static MethodInfo GetGathererCall(Type t)
        {
            var methods = typeof(GathererConnection).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(mi => mi.GetCustomAttribute<GathererCallAttribute>() != null).ToList();

            return methods.FirstOrDefault(mi => mi.GetCustomAttribute<GathererCallAttribute>().Type == t);
        }
    }
}
