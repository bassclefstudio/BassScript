using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BassClefStudio.BassScript.Data;

namespace BassClefStudio.BassScript.Runtime
{
    /// <summary>
    /// Provides a means for running and evaluating <see cref="IExpression"/>s within a given context. 
    /// </summary>
    public interface IExpressionRuntime
    {
        /// <summary>
        /// Executes the given <see cref="IExpression"/> and returns an <see cref="object"/> result.
        /// </summary>
        /// <param name="expression">The <see cref="IExpression"/> being run.</param>
        /// <param name="context">The <see cref="IRuntimeObject"/> which is the current context of the object where this <see cref="IExpression"/> is being evaluated.</param>
        Task<object?> ExecuteAsync(IExpression expression, IRuntimeObject context);
    }

    /// <summary>
    /// An <see cref="Exception"/> thrown when an <see cref="IExpressionRuntime"/> fails to evaluate a provided expression.
    /// </summary>
    [Serializable]
    public class RuntimeException : Exception
    {
        /// <summary>
        /// The <see cref="IExpression"/> that could not be evaluated.
        /// </summary>
        public IExpression Expression { get; }

        /// <summary>
        /// Creates a new <see cref="RuntimeException"/>.
        /// </summary>
        /// <param name="expression">The <see cref="IExpression"/> that could not be evaluated.</param>
        public RuntimeException(IExpression expression) { Expression = expression; }

        /// <summary>
        /// Creates a new <see cref="RuntimeException"/>.
        /// </summary>
        /// <param name="expression">The <see cref="IExpression"/> that could not be evaluated.</param>
        /// <param name="message">The <see cref="string"/> message to display for this <see cref="RuntimeException"/>.</param>
        public RuntimeException(IExpression expression, string message) : base(message) { Expression = expression; }

        /// <summary>
        /// Creates a new <see cref="RuntimeException"/>.
        /// </summary>
        /// <param name="expression">The <see cref="IExpression"/> that could not be evaluated.</param>
        /// <param name="message">The <see cref="string"/> message to display for this <see cref="RuntimeException"/>.</param>
        /// <param name="inner">The inner exception that is the cause of the current <see cref="RuntimeException"/>.</param>
        public RuntimeException(IExpression expression, string message, Exception inner) : base(message, inner) { Expression = expression; }
    }
}
