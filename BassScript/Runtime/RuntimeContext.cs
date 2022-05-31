using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.BassScript.Runtime
{
    /// <summary>
    /// Provides an <see cref="IRuntimeObject"/> which can serve as the current executing context of an <see cref="IExpressionRuntime"/>.
    /// </summary>
    public record RuntimeContext : IRuntimeObject
    {
        /// <summary>
        /// Provides the <see cref="IDictionary{TKey,TValue}"/> of property values shared by all related <see cref="RuntimeContext"/>s (core methods, constants, etc.).
        /// </summary>
        public IDictionary<string, object?> Core { get; }

        /// <summary>
        /// Provides an <see cref="IDictionary{TKey,TValue}"/> of local variable assignments which are not preserved during context switches.
        /// </summary>
        public IDictionary<string, object?> Local { get; }

        /// <summary>
        /// The current <see cref="IRuntimeObject"/> data context of the <see cref="RuntimeContext"/> (also known as 'this').
        /// </summary>
        public IRuntimeObject? Me { get; private set; }

        /// <inheritdoc/>
        public object? this[string key]
        {
            get
            {
                if (key == "this") return Me;
                if (Local.ContainsKey(key)) return Local[key];
                if (Core.ContainsKey(key)) return Core[key];
                if (Me is not null) return Me[key];
                throw new KeyNotFoundException($"Property \"{key}\" was not found in the current context.");
            }
            set => Local[key] = value;
        }
        
        /// <summary>
        /// Creates a new, empty <see cref="RuntimeContext"/>.
        /// </summary>
        public RuntimeContext()
        {
            Core = new Dictionary<string, object?>();
            Local = new Dictionary<string, object?>();
            Me = null;
        }

        /// <summary>
        /// Creates a new <see cref="RuntimeContext"/> with initialized values.
        /// </summary>
        /// <param name="core">Provides the <see cref="IDictionary{TKey,TValue}"/> of property values shared by all related <see cref="RuntimeContext"/>s (core methods, constants, etc.).</param>
        /// <param name="local">Provides an <see cref="IDictionary{TKey,TValue}"/> of local variable assignments which are not preserved during context switches.</param>
        /// <param name="me">The current <see cref="IRuntimeObject"/> data context of the <see cref="RuntimeContext"/> (also known as 'this').</param>
        private RuntimeContext(IDictionary<string, object?> core, IDictionary<string, object?> local, IRuntimeObject? me)
        {
            Core = core;
            Local = local;
            Me = me;
        }

        /// <summary>
        /// Sets the <see cref="RuntimeContext"/>'s data context ('this' property) to a new <see cref="IRuntimeObject"/>.
        /// </summary>
        /// <param name="me">The new <see cref="Me"/> property.</param>
        /// <param name="locals">Optionally, a set of <see cref="KeyValuePair{TKey,TValue}"/> local parameters to set in the new context.</param>
        /// <returns>The resulting <see cref="RuntimeContext"/>.</returns>
        public RuntimeContext SetSelf(IRuntimeObject? me, params KeyValuePair<string, object?>[] locals) 
            => new RuntimeContext(Core, new Dictionary<string, object?>(locals), me);
        
        /// <summary>
        /// Creates a copy of the <see cref="RuntimeContext"/> with a new local scope.
        /// </summary>
        /// <param name="locals">A set of <see cref="KeyValuePair{TKey,TValue}"/> local parameters to set in the new context.</param>
        /// <returns>The resulting <see cref="RuntimeContext"/>.</returns>
        public RuntimeContext Copy(params KeyValuePair<string, object?>[] locals)
            => new RuntimeContext(Core, new Dictionary<string, object?>(Local.Concat(locals)), Me);
    }
}
