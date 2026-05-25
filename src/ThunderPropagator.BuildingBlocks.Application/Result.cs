namespace ThunderPropagator.BuildingBlocks.Application
{
    /// <summary>
    /// Represents the outcome of an operation that either succeeds with a value
    /// or fails with an expected error message, forcing callers to check
    /// <see cref="IsSuccess"/> before accessing <see cref="Value"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value on success.</typeparam>
    public sealed class Result<T>
    {
        private readonly T? _value;

        private Result(T value)
        {
            _value = value;
            IsSuccess = true;
        }

        private Result(string error)
        {
            Error = error;
            IsSuccess = false;
        }

        /// <summary>Gets a value indicating whether the operation succeeded.</summary>
        public bool IsSuccess { get; }

        /// <summary>Gets the error message when the operation failed; <see langword="null"/> on success.</summary>
        public string? Error { get; }

        /// <summary>
        /// Gets the result value.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when accessed on a failed result. Check <see cref="IsSuccess"/> first.
        /// </exception>
        public T Value => IsSuccess ? _value! : throw new System.InvalidOperationException($"Cannot access Value of a failed Result. Error: {Error}");

        /// <summary>Creates a successful <see cref="Result{T}"/> containing <paramref name="value"/>.</summary>
        public static Result<T> Success(T value)
        {
            return new Result<T>(value);
        }

        /// <summary>Creates a failed <see cref="Result{T}"/> with the given <paramref name="error"/> message.</summary>
        public static Result<T> Failure(string error)
        {
            return new Result<T>(error);
        }
    }
}
