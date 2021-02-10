using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BassClefStudio.SymbolicMath.Core.Expressions
{
    /// <summary>
    /// An <see cref="IExpression"/> which calls another <see cref="IExpression"/> with a given set of <see cref="IExpression"/> inputs.
    /// </summary>
    public class FunctionCall : IExpression
    {
        /// <summary>
        /// The function or other <see cref="IExpression"/> that this <see cref="FunctionCall"/> calls.
        /// </summary>
        public IExpression Expression { get; }

        /// <summary>
        /// An ordered collection of <see cref="IExpression"/> inputs to the function.
        /// </summary>
        public IEnumerable<IExpression> Inputs { get; }

        /// <summary>
        /// Creates a new <see cref="FunctionCall"/>.
        /// </summary>
        /// <param name="expression">The function or other <see cref="IExpression"/> that this <see cref="FunctionCall"/> calls.</param>
        /// <param name="inputs">An ordered collection of <see cref="IExpression"/> inputs to the function.</param>
        public FunctionCall(IExpression expression, params IExpression[] inputs) : this(expression, inputs as IEnumerable<IExpression>)
        { }

        /// <summary>
        /// Creates a new <see cref="FunctionCall"/>.
        /// </summary>
        /// <param name="expression">The function or other <see cref="IExpression"/> that this <see cref="FunctionCall"/> calls.</param>
        /// <param name="inputs">An ordered collection of <see cref="IExpression"/> inputs to the function.</param>
        public FunctionCall(IExpression expression, IEnumerable<IExpression> inputs)
        {
            Expression = expression;
            Inputs = inputs;
        }

        /// <inheritdoc/>
        public bool Equals(IExpression other)
        {
            return other is FunctionCall call
                && this.Expression.Equals(call.Expression)
                && this.Inputs.SequenceEqual(call.Inputs);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Expression}({string.Join(",",Inputs)})";
        }
    }
}
