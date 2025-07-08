using System.Collections.Generic;
using System.Linq.Expressions;

namespace Audacia.Spreadsheets.Extensions.Helpers
{
    /// <summary>
    /// Expression visitor that replaces an expression with another of the same type, for use in chaining expressions.
    /// </summary>
    internal class ParameterRebinder : ExpressionVisitor
    {
        private readonly IDictionary<Expression, Expression> _map;

        private ParameterRebinder(Dictionary<Expression, Expression> map)
        {
            _map = map ?? new Dictionary<Expression, Expression>();
        }

        /// <summary>
        /// Replace the parameters in an expression with the specified map.
        /// </summary>
        /// <param name="map">A map from the old expression to the expression to replace it with.</param>
        /// <param name="exp">The expression to perform the rebinding on.</param>
        /// <returns>The new expression after rebinding.</returns>
        public static Expression ReplaceParameters(
            Dictionary<Expression, Expression> map,
            Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }

        /// <inheritdoc />
        protected override Expression VisitParameter(ParameterExpression expression)
        {
            return _map.TryGetValue(expression, out var replacement)
                ? replacement
                : base.VisitParameter(expression);
        }
    }
}