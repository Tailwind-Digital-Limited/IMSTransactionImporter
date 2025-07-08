using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Audacia.Spreadsheets.Extensions.Helpers;

namespace Audacia.Core.Extensions
{
    /// <summary>
    /// Extension methods for the type <see cref="Expression"/>.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Return property information for a lambda property expression.
        /// </summary>
        /// <param name="propertyExpression">A lambda property expression.</param>
        /// <returns>Property information for <paramref name="propertyExpression"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="propertyExpression"/> is not a lambda expression.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="propertyExpression"/> does not have a declaring type.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="propertyExpression"/> is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Maintainability",
            "AV1551:Method overload should call another overload",
            Justification = "This is called from another overload.")]
        public static PropertyInfo? GetPropertyInfo(Expression propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            if (propertyExpression.NodeType != ExpressionType.Lambda)
            {
                throw new ArgumentException("Selector must be lambda expression", nameof(propertyExpression));
            }

            var lambda = (LambdaExpression)propertyExpression;

            var memberExpression = ExtractMemberExpression(lambda.Body);

            if (memberExpression == null)
            {
                throw new ArgumentException("Selector must be member access expression", nameof(propertyExpression));
            }

            if (memberExpression.Member.DeclaringType == null)
            {
                throw new InvalidOperationException("Property does not have declaring type");
            }

            return memberExpression.Member.DeclaringType.GetProperty(memberExpression.Member.Name);
        }

        /// <summary>
        /// Return property information for a lambda property expression.
        /// </summary>
        /// <typeparam name="T">The type of the property information.</typeparam>
        /// <param name="obj">Source object to return type argument implicitly.</param>
        /// <param name="propertyExpression">A lambda property expression.</param>
        /// <returns>Property information for <paramref name="propertyExpression"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="propertyExpression"/> is not a lambda expression.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="propertyExpression"/> does not have a declaring type.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="propertyExpression"/> is null.</exception>
        public static PropertyInfo? GetPropertyInfo<T>(this T obj, Expression<Func<T, object>> propertyExpression)
        {
            //Overloaded so allow object specific property access and allow implicit type argument
            return GetPropertyInfo(propertyExpression);
        }

        private static MemberExpression ExtractMemberExpression(Expression expression)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return (MemberExpression)expression;
                case ExpressionType.Convert:
                    var operand = ((UnaryExpression)expression).Operand;
                    return ExtractMemberExpression(operand);
                default:
                    return default!;
            }
        }

        /// <summary>
        /// Convert generic type argument in an expression to a specified type.
        /// </summary>
        /// <typeparam name="TSource">The generic type.</typeparam>
        /// <typeparam name="TTarget">The type to replace <typeparamref name="TSource"/>.</typeparam>
        /// <typeparam name="TResult">The returned value.</typeparam>
        /// <param name="root">The original expression.</param>
        /// <returns>An expression with <typeparamref name="TSource"/> replaced with <typeparamref name="TTarget"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Maintainability",
            "AV1551:Method overload should call another overload",
            Justification = "Other method with same name isn't the same functionality.")]
        public static Expression<Func<TTarget, TResult>> ConvertGenericTypeArgument<TSource, TTarget, TResult>(
            this Expression<Func<TSource, TResult>> root)
        {
            var visitor = new ParameterReplacer(typeof(TSource), typeof(TTarget));
            return (visitor.Visit(root) as Expression<Func<TTarget, TResult>>) ?? default!;
        }

        /// <summary>
        /// Convert generic type argument in an expression to a specified type.
        /// </summary>
        /// <typeparam name="TSource">The generic type.</typeparam>
        /// <typeparam name="TResult">The returned value.</typeparam>
        /// <param name="root">The original expression.</param>
        /// <param name="targetType">The type to replace <typeparamref name="TSource"/>.</param>
        /// <returns>An expression with <typeparamref name="TSource"/> replaced with <paramref name="targetType"/>.</returns>
        public static LambdaExpression ConvertGenericTypeArgument<TSource, TResult>(
            this Expression<Func<TSource, TResult>> root, Type targetType)
        {
            var visitor = new ParameterReplacer(typeof(TSource), targetType);
            var expression = visitor.Visit(root);
            return (expression as LambdaExpression) ?? default!;
        }

        /// <summary>
        /// Compiles an expression.
        /// </summary>
        /// <typeparam name="T">The type of argument.</typeparam>
        /// <typeparam name="TResult">The type of the returned value.</typeparam>
        /// <param name="expression">The expression to compile.</param>
        /// <param name="arg">An argument provided for the expression.</param>
        /// <returns><typeparamref name="TResult"/> result from the <paramref name="expression"/>."/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is null.</exception>
        public static TResult Execute<T, TResult>(this Expression<Func<T, TResult>> expression, T arg)
        {
            return (expression ?? throw new ArgumentNullException(nameof(expression), "Expression cannot be null"))
                .Compile()(arg);
        }

        /// <summary>
        /// Compiles an expression.
        /// </summary>
        /// <typeparam name="T">The type of argument.</typeparam>
        /// <param name="expression">The expression to compile.</param>
        /// <param name="arg">An argument provided for the expression.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is null.</exception>
        public static void Perform<T>(this Expression<Action<T>> expression, T arg)
        {
            (expression ?? throw new ArgumentNullException(nameof(expression), "Expression cannot be null"))
                .Compile()(arg);
        }

        /// <summary>
        /// Combines the first predicate with the second using the logical "and".
        /// </summary>
        /// <param name="first">The source expression.</param>
        /// <param name="second">The expression to join on to <paramref name="first"/>.</param>
        /// <typeparam name="T">The input type.</typeparam>
        /// <returns>An expression combining <paramref name="first"/> and <paramref name="second"/>.</returns>
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        /// <summary>
        /// Combines the first predicate with the second using the logical "or".
        /// </summary>
        /// <param name="first">The source expression.</param>
        /// <param name="second">The expression to join on to <paramref name="first"/>.</param>
        /// <typeparam name="T">The input type.</typeparam>
        /// <returns>An expression combining <paramref name="first"/> or <paramref name="second"/>.</returns>
        public static Expression<Func<T, bool>> Or<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        /// <summary>
        /// Negates the predicate.
        /// </summary>
        /// <param name="expression">The source expression.</param>
        /// <typeparam name="T">The input type.</typeparam>
        /// <returns>An expression with a negated prediction.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is null.</exception>
        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var negated = Expression.Not(expression.Body);
            return Expression.Lambda<Func<T, bool>>(negated, expression.Parameters);
        }

        /// <summary>
        /// Apply the <paramref name="second"/> expression to the result of this expression.
        /// </summary>
        /// <typeparam name="TIn">The input type of the source expression.</typeparam>
        /// <typeparam name="TInter">The output type of the source expression and the input type of the return expression.</typeparam>
        /// <typeparam name="TOut">The output type of the return expression.</typeparam>
        /// <param name="first">The source expression.</param>
        /// <param name="second">The second expression to apply to the result of the first.</param>
        /// <returns>An expression to run <paramref name="second"/> after <paramref name="first"/> expression.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is null.</exception>
        public static Expression<Func<TIn, TOut>> Then<TIn, TInter, TOut>(
            this Expression<Func<TIn, TInter>> first,
            Expression<Func<TInter, TOut>> second)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }

            //Map the parameters of the second expression to the body of the first
            var replacements = second.Parameters
                .Select(p => new { parameter = p, replacement = first.Body })
                .ToDictionary(p => (Expression)p.parameter, p => p.replacement);

            //Replace the parameters of the second Expression with the body of the first
            var secondBody = ParameterRebinder.ReplaceParameters(replacements, second.Body);
            return Expression.Lambda<Func<TIn, TOut>>(secondBody, first.Parameters);
        }

        /// <summary>
        /// Merges two expressions into one.
        /// </summary>
        /// <typeparam name="T">The input type of the merging expressions.</typeparam>
        /// <param name="first">The source expression.</param>
        /// <param name="second">The expression to merge with <paramref name="first"/>.</param>
        /// <param name="merge">How the expressions are merged together.</param>
        /// <returns>The merged expression of <paramref name="first"/> and <paramref name="second"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is null.</exception>
        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second,
            Func<Expression, Expression, Expression> merge)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }

            if (merge == null)
            {
                throw new ArgumentNullException(nameof(merge));
            }

            // zip parameters (map from parameters of second to parameters of first)
            var map = first.Parameters
                .Select((f, i) => new { first = (Expression)f, second = (Expression)second.Parameters[i] })
                .ToDictionary(p => p.second, p => p.first);

            // replace parameters in the second lambda expression with the parameters in the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // create a merged lambda expression with parameters from the first expression
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }
    }
}