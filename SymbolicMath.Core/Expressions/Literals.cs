using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.SymbolicMath.Core.Expressions
{
    /// <summary>
    /// Represents a variable or function declaration of any kind.
    /// </summary>
    public class Identifier : IExpression
    {
        /// <summary>
        /// The name of the <see cref="Identifier"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a new <see cref="Identifier"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="Identifier"/>.</param>
        public Identifier(string name)
        {
            Name = name;
        }

        /// <inheritdoc/>
        public bool Equals(IExpression other)
        {
            return other is Identifier i
                && this.Name == i.Name;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Represents an <see cref="IExpression"/> of an integer (whole-number) value.
    /// </summary>
    public class IntegerExpression : IExpression
    {
        /// <summary>
        /// The stored <see cref="int"/> value.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Creates a new <see cref="IntegerExpression"/>.
        /// </summary>
        /// <param name="value">The stored <see cref="int"/> value.</param>
        public IntegerExpression(int value)
        {
            Value = value;
        }

        /// <inheritdoc/>
        public bool Equals(IExpression other)
        {
            return other is IntegerExpression i &&
                this.Value == i.Value;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    /// <summary>
    /// Represents an <see cref="IExpression"/> of an double-precision numerical value.
    /// </summary>
    public class DoubleExpression : IExpression
    {
        /// <summary>
        /// The stored <see cref="double"/> value.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Creates a new <see cref="IntegerExpression"/>.
        /// </summary>
        /// <param name="value">The stored <see cref="double"/> value.</param>
        public DoubleExpression(double value)
        {
            Value = value;
        }

        /// <inheritdoc/>
        public bool Equals(IExpression other)
        {
            return other is DoubleExpression i &&
                this.Value == i.Value;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    /// <summary>
    /// Represents an <see cref="IExpression"/> of an double-precision numerical value.
    /// </summary>
    public class StringExpression : IExpression
    {
        /// <summary>
        /// The stored <see cref="double"/> value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Creates a new <see cref="IntegerExpression"/>.
        /// </summary>
        /// <param name="value">The stored <see cref="string"/> value.</param>
        public StringExpression(string value)
        {
            Value = value;
        }

        /// <inheritdoc/>
        public bool Equals(IExpression other)
        {
            return other is StringExpression i &&
                this.Value == i.Value;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
