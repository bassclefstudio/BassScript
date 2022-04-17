namespace BassClefStudio.SymbolicLanguage.Data
{
    /// <summary>
    /// Represents a basic token of any type.
    /// </summary>
    public interface IExpression : IEquatable<IExpression>
    { }

    /// <summary>
    /// Represents an <see cref="IExpression"/> which encapsulates some <typeparamref name="T"/> data.
    /// </summary>
    /// <typeparam name="T">The type of data this <see cref="IExpression{T}"/> holds.</typeparam>
    public interface IExpression<out T> : IExpression
    {
        /// <summary>
        /// The <typeparamref name="T"/> value contained within this <see cref="IExpression{T}"/>.
        /// </summary>
        T Value { get; }
    }
}