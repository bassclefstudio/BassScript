using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BassClefStudio.BassScript.Data;

namespace BassClefStudio.BassScript.Runtime
{
    /// <summary>
    /// Represents a complex data object held within an <see cref="IExpression"/>, which contains one or more data fields or methods.
    /// </summary>
    public interface IRuntimeObject
    {
        /// <summary>
        /// Gets or sets the given data object associated with a <see cref="string"/> key.
        /// </summary>
        /// <param name="key">The unique <see cref="string"/> key identifying the object in question.</param>
        /// <returns>The resulting <see cref="object"/> data or <see cref="RuntimeMethod"/> method.</returns>
        object? this[string key] { get; set; }
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
