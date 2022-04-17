using BassClefStudio.SymbolicLanguage.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.SymbolicLanguage.Runtime
{
    /// <summary>
    /// Represents a complex data object held within an <see cref="IExpression"/>, which contains one or more data fields or methods.
    /// </summary>
    public interface IRuntimeObject
    {
        /// <summary>
        /// An <see cref="IDictionary{TKey, TValue}"/> of data fields keyed with <see cref="string"/> names.
        /// </summary>
        IDictionary<string, object?> Fields { get; }

        /// <summary>
        /// An <see cref="IDictionary{TKey, TValue}"/> of methods which can take a set of input <see cref="object"/>s and return an output <see cref="object"/>, keyed with <see cref="string"/> names.
        /// </summary>
        IDictionary<string, RuntimeMethod> Methods { get; }

        /// <summary>
        /// Gets the given field or method (see <see cref="Fields"/> and <see cref="Methods"/>) associated with the given <see cref="string"/> key.
        /// </summary>
        /// <param name="key">The unique <see cref="string"/> key identifying the object in question.</param>
        /// <returns>The resulting <see cref="object"/> data or <see cref="RuntimeMethod"/> method.</returns>
        object? this[string key] { get; }
    }

    /// <summary>
    /// Represents an asynchronous method which can be a member of an <see cref="IRuntimeObject"/>.
    /// </summary>
    /// <param name="args">The collection of <see cref="object"/> parameters which are provided as inputs.</param>
    /// <returns>The output of the method as an <see cref="object"/>.</returns>
    public delegate Task<object?> RuntimeMethod(object?[] args);

    /// <summary>
    /// Represents an action which can bind a given variable declaration to a provided <see cref="IRuntimeObject"/>.
    /// </summary>
    /// <param name="me">The context <see cref="IRuntimeObject"/> ('this') being bound to.</param>
    public delegate void VarBinding(IRuntimeObject me);
}
