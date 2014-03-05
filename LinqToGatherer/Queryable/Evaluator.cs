using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToGatherer
{
    public static class Evaluator
    {
        /// <summary>
        /// Returns a new expression where variable values are replaced with the ConstantExpression values
        /// <para>
        /// i.e. c.Set == setName -> c.Set == "Thereos"
        /// </para>
        /// </summary>
        public static Expression PartialEval(Expression expression, Func<Expression, bool> canBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(canBeEvaluated).Nominate(expression)).Eval(expression);
        }

        /// <summary>
        /// Returns a new expression where variable values are replaced with the ConstantExpression values
        /// <para>
        /// i.e. c.Set == setName -> c.Set == "Thereos"
        /// </para>
        /// </summary>
        public static Expression PartialEval(Expression expression)
        {
            return PartialEval(expression, e => e.NodeType != ExpressionType.Parameter);
        }

        internal class SubtreeEvaluator : ExpressionVisitor
        {
            private HashSet<Expression> candidates;

            internal SubtreeEvaluator(HashSet<Expression> candidates)
            {
                this.candidates = candidates;
            }

            internal Expression Eval(Expression expression)
            {
                return this.Visit(expression);
            }

            public override Expression Visit(Expression node)
            {
                if (node == null)
                    return null;

                if (this.candidates.Contains(node))
                    return this.Evaluate(node);

                return base.Visit(node);
            }

            Expression Evaluate(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant)
                    return e;

                var lambda = Expression.Lambda(e);

                var fn = lambda.Compile();

                return Expression.Constant(fn.DynamicInvoke(null), e.Type);
            }
        }

        /// <summary>
        /// Determines which nodes in the expression tree can be evaluated right now
        /// </summary>
        internal class Nominator : ExpressionVisitor
        {
            private Func<Expression, bool> canBeEvaluated;
            private HashSet<Expression> candidates;
            private bool cannotBeEvaluated;

            internal Nominator(Func<Expression, bool> canBeEvaluated)
            {
                this.canBeEvaluated = canBeEvaluated;
            }

            internal HashSet<Expression> Nominate(Expression expression)
            {
                this.candidates = new HashSet<Expression>();

                this.Visit(expression);

                return this.candidates;
            }

            public override Expression Visit(Expression node)
            {
                if (node != null)
                {
                    var localCannotBeEvaluated = this.cannotBeEvaluated;
                    this.cannotBeEvaluated = false;

                    base.Visit(node);

                    if (!this.cannotBeEvaluated)
                    {
                        if (canBeEvaluated(node))
                            this.candidates.Add(node);
                        else
                            this.cannotBeEvaluated = true;
                    }

                    this.cannotBeEvaluated |= localCannotBeEvaluated;
                }

                return node;
            }
        }
    }
}
