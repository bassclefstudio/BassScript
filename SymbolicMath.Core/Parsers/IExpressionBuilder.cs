using BassClefStudio.SymbolicMath.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.SymbolicMath.Core.Parsers
{
    /// <summary>
    /// Represents any service that can build an <see cref="IExpression"/> tree from a <typeparamref name="T"/> input.
    /// </summary>
    /// <typeparam name="T">The type of input this <see cref="IExpressionBuilder{T}"/> accepts.</typeparam>
    public interface IExpressionBuilder<in T>
    {
        /// <summary>
        /// Builds the <see cref="IExpression"/> encoded or represented by the <typeparamref name="T"/> input.
        /// </summary>
        /// <param name="input">The <typeparamref name="T"/> content to parse.</param>
        /// <returns>An <see cref="IExpression"/> containing the mathematical equivalent of <paramref name="input"/>.</returns>
        IExpression BuildExpression(T input);
    }
}
