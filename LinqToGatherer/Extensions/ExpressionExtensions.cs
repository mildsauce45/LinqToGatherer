using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace LinqToGatherer
{
    public static class ExpressionExtensions
    {
        #region Equals Expressions

        internal static bool IsMemberValueExpression<T>(this Expression e, string propertyName)
        {
            var declaringType = typeof(T);

            var be = e as BinaryExpression;

            if (be.Left.IsSpecificMemberExpression(declaringType, propertyName) && be.Right.IsSpecificMemberExpression(declaringType, propertyName))
                throw new Exception("Cannot have 'property' == 'property' in an expression.");

            return be.Left.IsSpecificMemberExpression(declaringType, propertyName) || be.Right.IsSpecificMemberExpression(declaringType, propertyName);
        }

        internal static bool IsSpecificMemberExpression(this Expression e, Type declaringType, string propertyName)
        {
            if (e is MemberExpression)
                return (e as MemberExpression).Member.DeclaringType == declaringType && (e as MemberExpression).Member.Name == propertyName;
            else if (e is UnaryExpression)
            {
                var u = e as UnaryExpression;
                var me = u.Operand as MemberExpression;

                return me != null && me.Member.DeclaringType == declaringType && me.Member.Name == propertyName;
            }

            return false;
        }

        internal static R GetValueFromExpression<T, R>(this Expression e, string propertyName)
        {
            var declaringType = typeof(T);

            var be = e as BinaryExpression;

            if (be.Left.NodeType == ExpressionType.MemberAccess)
            {
                var me = be.Left as MemberExpression;

                if (me.Member.DeclaringType == declaringType && me.Member.Name == propertyName)
                    return GetValueFromExpression<R>(be.Right);
            }
            else if (be.Right.NodeType == ExpressionType.MemberAccess)
            {
                var me = be.Right as MemberExpression;

                if (me.Member.DeclaringType == declaringType && me.Member.Name == propertyName)
                    return GetValueFromExpression<R>(be.Left);
            }
            else if (be.Left.NodeType == ExpressionType.Convert)
            {
                var me = (be.Left as UnaryExpression).Operand as MemberExpression;

                if (me.Member.DeclaringType == declaringType && me.Member.Name == propertyName)
                    return GetValueFromExpression<R>(be.Right);
            }
            else if (be.Right.NodeType == ExpressionType.Convert)
            {
                var me = (be.Right as UnaryExpression).Operand as MemberExpression;

                if (me.Member.DeclaringType == declaringType && me.Member.Name == propertyName)
                    return GetValueFromExpression<R>(be.Left);
            }

            throw new Exception("Something else really bad just happened here.");
        }

        internal static R GetValueFromExpression<R>(this Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
                return (R)(expression as ConstantExpression).Value;
            else
                throw new Exception(string.Format("The expression type {0} is not supported for retrieving values.", expression.NodeType));
        }

        #endregion
    }
}
