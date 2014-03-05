using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqToGatherer.Queryable.Contracts;
using LinqToGatherer.Utility;
using System.Collections;

namespace LinqToGatherer
{
    internal class UrlParameterBuilder<T> : ExpressionVisitor
    {
        private INodeTypeVisitor<T> equalsVisitor;
        private INodeTypeVisitor<T> notEqualsVisitor;
        private INodeTypeVisitor<T> lessThanVisitor;
        private INodeTypeVisitor<T> lessThanOrEqualVisitor;
        private INodeTypeVisitor<T> greaterThanVisitor;
        private INodeTypeVisitor<T> greaterThanOrEqualVisitor;

        public IList<PropertyQueryInfo> Parameters { get; private set; }

        public UrlParameterBuilder()
        {
            Parameters = new List<PropertyQueryInfo>();

            CreateVisitors();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if ((node.NodeType == ExpressionType.Equal && equalsVisitor != null && equalsVisitor.Visit(node, Parameters)) ||
                (node.NodeType == ExpressionType.NotEqual && notEqualsVisitor != null && notEqualsVisitor.Visit(node, Parameters)) ||
                (node.NodeType == ExpressionType.LessThan && lessThanVisitor != null && lessThanVisitor.Visit(node, Parameters)) ||
                (node.NodeType == ExpressionType.LessThanOrEqual && lessThanOrEqualVisitor != null && lessThanOrEqualVisitor.Visit(node, Parameters)) ||
                (node.NodeType == ExpressionType.GreaterThan && greaterThanVisitor != null && greaterThanVisitor.Visit(node, Parameters)) ||
                (node.NodeType == ExpressionType.GreaterThanOrEqual && greaterThanOrEqualVisitor != null && greaterThanOrEqualVisitor.Visit(node, Parameters)))
            {
                return node;
            }

            return base.VisitBinary(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var allowedMethods = new string[] { "StartsWith", "Contains", "EndsWith" };

            if (node.Method.DeclaringType == typeof(string) && allowedMethods.Contains(node.Method.Name))
            {
                if (node.Object.IsSpecificMemberExpression(typeof(Card), "Name"))
                    Parameters.Add(new PropertyQueryInfo("name", node.Arguments[0].GetValueFromExpression<string>()));
                else if (node.Object.IsSpecificMemberExpression(typeof(Card), "Description"))
                    Parameters.Add(new PropertyQueryInfo("text", node.Arguments[0].GetValueFromExpression<string>()));
                else if (node.Object.IsSpecificMemberExpression(typeof(Card), "SubTypes"))
                    Parameters.Add(new PropertyQueryInfo("subtype", node.Arguments[0].GetValueFromExpression<string>()));
                else if (node.Object.IsSpecificMemberExpression(typeof(Card), "Set"))
                    Parameters.Add(new PropertyQueryInfo("set", node.Arguments[0].GetValueFromExpression<string>()));
            }
            else if (node.Method.Name == "Contains")
            {
                // Support the construct of list.Contains(...)
                if (node.Object is ConstantExpression)
                {
                    var list = (node.Object as ConstantExpression).Value as IEnumerable;
                    var propName = (node.Arguments[0] as MemberExpression).Member.Name;

                    foreach (var obj in list)
                        Parameters.Add(new PropertyQueryInfo(propName.ToLower(), obj.ToString(), wrapInQuotes: obj.GetType() == typeof(string)));
                }
            }

            return base.VisitMethodCall(node);
        }

        private void CreateVisitors()
        {
            var type = typeof(T);

            var ntvType = typeof(INodeTypeVisitor<>);

            var possibleTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Implements(ntvType) && t != ntvType);

            foreach (var pt in possibleTypes)
            {
                // If this node visitor is for the proper type than we can set it to the proper field
                if (pt.GetInterface("INodeTypeVisitor`1").GetUnderlyingType() == type)
                {
                    var nodeTypeAttribute = pt.GetCustomAttribute<NodeTypeAttribute>();

                    if (nodeTypeAttribute != null)
                    {
                        var visitor = Activator.CreateInstance(pt) as INodeTypeVisitor<T>;

                        // This could probably be made more concise by allowing the node type attribute to be applied to the private field
                        // Thus allowing us to match the visitor to the field when setting it. For now though, just to get this all working
                        // I'm going to directly assign based on a switch statement
                        switch (nodeTypeAttribute.NodeType)
                        {
                            case ExpressionType.Equal:
                                equalsVisitor = visitor;
                                break;
                            case ExpressionType.NotEqual:
                                notEqualsVisitor = visitor;
                                break;
                            case ExpressionType.LessThan:
                                lessThanVisitor = visitor;
                                break;
                            case ExpressionType.LessThanOrEqual:
                                lessThanOrEqualVisitor = visitor;
                                break;
                            case ExpressionType.GreaterThan:
                                greaterThanVisitor = visitor;
                                break;
                            case ExpressionType.GreaterThanOrEqual:
                                greaterThanOrEqualVisitor = visitor;
                                break;
                        }
                    }
                }
            }
        }
    }
}
