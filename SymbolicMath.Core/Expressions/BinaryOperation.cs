using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.SymbolicMath.Core.Expressions
{
    /// <summary>
    /// Represents an <see cref="IExpression"/> which applies an <see cref="UnaryOperator"/> to a given <see cref="IExpression"/>.
    /// </summary>
    public class BinaryOperation : IExpression
    {
        /// <summary>
        /// The first input to this <see cref="BinaryOperation"/>.
        /// </summary>
        public IExpression ArgA { get; }

        /// <summary>
        /// The second input to this <see cref="BinaryOperation"/>.
        /// </summary>
        public IExpression ArgB { get; }

        /// <summary>
        /// The <see cref="BinaryOperator"/> value describing the type of operation.
        /// </summary>
        public BinaryOperator Operator { get; }

        /// <summary>
        /// Creates a new <see cref="BinaryOperation"/>.
        /// </summary>
        /// <param name="a">The first input to this <see cref="BinaryOperation"/>.</param>
        /// <param name="b">The second input to this <see cref="BinaryOperation"/>.</param>
        /// <param name="opType">The <see cref="BinaryOperator"/> value describing the type of operation.</param>
        public BinaryOperation(IExpression a, IExpression b, BinaryOperator opType)
        {
            ArgA = a;
            ArgB = b;
            Operator = opType;
        }

        /// <inheritdoc/>
        public bool Equals(IExpression other)
        {
            return other is BinaryOperation op
                && ArgA.Equals(op.ArgA)
                && ArgB.Equals(op.ArgB)
                && Operator == op.Operator;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{{{Operator} {ArgA},{ArgB}}}";
        }
    }

    /// <summary>
    /// An enum defining the types of binary (two-input) operators and their behavior.
    /// </summary>
    public enum BinaryOperator
    {
        /// <summary>
        /// Sum two values together.
        /// </summary>
        Add = 0,
        /// <summary>
        /// Multiply two values together.
        /// </summary>
        Multiply = 1,
        /// <summary>
        /// Find the difference between two values (subtract the second from the first).
        /// </summary>
        Subtract = 2,
        /// <summary>
        /// Divide the first value by the second value.
        /// </summary>
        Divide = 3,
    }
}
