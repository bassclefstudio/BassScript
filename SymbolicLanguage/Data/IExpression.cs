namespace BassClefStudio.SymbolicLanguage.Data
{
    /// <summary>
    /// Represents a basic token of any type.
    /// </summary>
    public interface IExpression : IEquatable<IExpression>
    { }

    /// <summary>
    /// Represents an <see cref="IExpression"/> which encapsulates some literal data.
    /// </summary>
    public interface ILiteral : IExpression
    {
        /// <summary>
        /// Gets the <see cref="object"/> data contained within this <see cref="ILiteral"/>.
        /// </summary>
        object? GetValue();
    }
}