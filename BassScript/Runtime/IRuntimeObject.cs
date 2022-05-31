﻿using System;
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
        /// <exception cref="KeyNotFoundException">The provided <paramref name="key"/> was not found in the <see cref=" IRuntimeObject"/>'s properties.</exception>
        object? this[string key] { get; set; }
    }

    /// <summary>
    /// Represents an asynchronous method which can be a member of an <see cref="IRuntimeObject"/>.
    /// </summary>
    /// <param name="context">The <see cref="RuntimeContext"/> used as the runtime context for where this <see cref="RuntimeMethod"/> is executed.</param>
    /// <param name="args">The collection of <see cref="object"/> parameters which are provided as inputs.</param>
    /// <returns>The output of the method as an <see cref="object"/>.</returns>
    public delegate Task<object?> RuntimeMethod(RuntimeContext context, params object?[] args);

    /// <summary>
    /// Represents a compiled variable setter that can be passed between contexts. 
    /// </summary>
    /// <param name="context">The <see cref="RuntimeContext"/> where this <see cref="DefBinding"/> applies.</param>
    public delegate void DefBinding(RuntimeContext context);
}
