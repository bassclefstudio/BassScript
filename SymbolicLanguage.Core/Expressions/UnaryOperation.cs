using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.SymbolicLanguage.Expressions
{
    /// <summary>
    /// Represents an <see cref="IExpression"/> which applies an <see cref="UnaryOperator"/> to a given <see cref="IExpression"/>.
    /// </summary>
    public class UnaryOperation : IExpression
    {
        /// <summary>
        /// The input value to this <see cref="UnaryOperation"/>.
        /// </summary>
        public IExpression Arg { get; }

        /// <summary>
        /// The <see cref="UnaryOperator"/> value describing the type of operation.
        /// </summary>
        public UnaryOperator Operator { get; }

        /// <summary>
        /// Creates a new <see cref="UnaryOperation"/>.
        /// </summary>
        /// <param name="arg">The input value to this <see cref="UnaryOperation"/>.</param>
        /// <param name="opType">The <see cref="UnaryOperator"/> value describing the type of operation.</param>
        public UnaryOperation(IExpression arg, UnaryOperator opType)
        {
            Arg = arg;
            Operator = opType;
        }

        /// <inheritdoc/>
        public bool Equals(IExpression other)
        {
            return other is UnaryOperation op
                && Arg.Equals(op.Arg)
                && Operator == op.Operator;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{{{Operator} {Arg}}}";
        }
    }

    /// <summary>
    /// An enum defining the types of unary (single-input) operators and their behavior.
    /// </summary>
    public enum UnaryOperator
    {
        /// <summary>
        /// Gets the negative of the value of the operand.
        /// </summary>
        Negative = 0,
        /// <summary>
        /// Get the inverse boolean value of the operand.
        /// </summary>
        Not = 1
    }
}
