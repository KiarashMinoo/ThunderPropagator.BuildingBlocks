using Ardalis.GuardClauses;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class GuardClauseHelper
    {
        public static T GreaterThan<T>(
            this IGuardClause guardClause,
            T input,
            T indicator,
            [CallerArgumentExpression("input")] string? parameterName = null,
            string? message = null)
            where T : INumber<T>
        {
            return input > indicator ? input : throw new ArgumentException(message ?? $"Required input {parameterName} cannot be less than {indicator}.", parameterName);
        }

        public static T GreaterThan<T>(this T input, T indicator, [CallerArgumentExpression("input")] string? parameterName = null, string? message = null)
            where T : INumber<T>
        {
            return Guard.Against.GreaterThan(input, indicator, parameterName, message);
        }

        public static T GreaterThanOrEqual<T>(
            this IGuardClause guardClause,
            T input,
            T indicator,
            [CallerArgumentExpression("input")] string? parameterName = null,
            string? message = null)
            where T : INumber<T>
        {
            return input >= indicator ? input : throw new ArgumentException(message ?? $"Required input {parameterName} cannot be less than or equal {indicator}.", parameterName);
        }

        public static T GreaterThanOrEqual<T>(
            this T input,
            T indicator,
            [CallerArgumentExpression("input")] string? parameterName = null,
            string? message = null)
            where T : INumber<T>
        {
            return Guard.Against.GreaterThanOrEqual(input, indicator, parameterName, message);
        }

        public static T LessThan<T>(
            this IGuardClause guardClause,
            T input,
            T indicator,
            [CallerArgumentExpression("input")] string? parameterName = null,
            string? message = null)
            where T : INumber<T>
        {
            return input < indicator ? input : throw new ArgumentException(message ?? $"Required input {parameterName} cannot be greater than {indicator}.", parameterName);
        }

        public static T LessThan<T>(
            this T input,
            T indicator,
            [CallerArgumentExpression("input")] string? parameterName = null,
            string? message = null)
            where T : INumber<T>
        {
            return Guard.Against.LessThan(input, indicator, parameterName, message);
        }

        public static T LessThanOrEqual<T>(
            this IGuardClause guardClause,
            T input,
            T indicator,
            [CallerArgumentExpression("input")] string? parameterName = null,
            string? message = null)
            where T : INumber<T>
        {
            return input <= indicator
                ? input
                : throw new ArgumentException(message ?? $"Required input {parameterName} cannot be greater than or equal {indicator}.", parameterName);
        }

        public static T LessThanOrEqual<T>(
            this T input,
            T indicator,
            [CallerArgumentExpression("input")] string? parameterName = null,
            string? message = null)
            where T : INumber<T>
        {
            return Guard.Against.LessThanOrEqual(input, indicator, parameterName, message);
        }

        public static string MinLength(
            this IGuardClause guardClause,
            string input,
            int size,
            [CallerArgumentExpression("input")] string? parameterName = null,
            string? message = null)
        {
            return input.Length >= size ? input : throw new ArgumentException(message ?? $"Required input {parameterName} length cannot be less than {size}.", parameterName);
        }

        public static string MinLength(
            string input,
            int size,
            [CallerArgumentExpression("input")] string? parameterName = null,
            string? message = null)
        {
            return Guard.Against.MinLength(input, size, parameterName, message);
        }

        public static string MaxLength(
            this IGuardClause guardClause,
            string input,
            int size,
            [CallerArgumentExpression("input")] string? parameterName = null,
            string? message = null)
        {
            return input.Length <= size ? input : throw new ArgumentException(message ?? $"Required input {parameterName} length cannot be greater than {size}.", parameterName);
        }

        public static string MaxLength(
            this string input,
            int size,
            [CallerArgumentExpression("input")] string? parameterName = null,
            string? message = null)
        {
            return Guard.Against.MaxLength(input, size, parameterName, message);
        }

        public static string MeetRegex(
            this IGuardClause guardClause,
            string input,
            Regex regex,
            [CallerArgumentExpression("input")] string? parameterName = null,
            string? message = null)
        {
            return regex.Match(input).Success ? input : throw new ArgumentException(message ?? $"Required input {parameterName} does not meet regex {regex}.", parameterName);
        }

        public static string MeetRegex(
            this string input,
            Regex regex,
            [CallerArgumentExpression("input")] string? parameterName = null,
            string? message = null)
        {
            return Guard.Against.MeetRegex(input, regex, parameterName, message);
        }
    }
}