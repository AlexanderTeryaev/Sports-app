using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataService.Utils
{
    public static class ExpressionExtension
    {
        internal class SubstExpressionVisitor : ExpressionVisitor
        {
            private Dictionary<Expression, Expression> _subst;

            public Dictionary<Expression, Expression> Substitutions
            {
                get { return _subst ?? (_subst = new Dictionary<Expression, Expression>()); }
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                Expression newValue;
                return _subst.TryGetValue(node, out newValue) ? newValue : node;
            }
        }
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
        {
            if (a == null)
                return b;

            if (b == null)
                return a;
            var p = a.Parameters[0];

            var visitor = new SubstExpressionVisitor();
            visitor.Substitutions[b.Parameters[0]] = p;

            Expression body = Expression.AndAlso(a.Body, visitor.Visit(b.Body));
            return Expression.Lambda<Func<T, bool>>(body, p);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
        {
            if (a == null)
                return b;

            if (b == null)
                return a;
            var p = a.Parameters[0];

            var visitor = new SubstExpressionVisitor();
            visitor.Substitutions[b.Parameters[0]] = p;

            Expression body = Expression.OrElse(a.Body, visitor.Visit(b.Body));
            return Expression.Lambda<Func<T, bool>>(body, p);
        }
    }
}