using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassClefStudio.SymbolicLanguage.Runtime
{
    /// <summary>
    /// Provides an <see cref="IRuntimeObject"/> which can serve as the current executing context of an <see cref="IExpressionRuntime"/>.
    /// </summary>
    public record RuntimeContext : IRuntimeObject
    {
        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, object?> Core { get; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, object?> Local { get; }

        /// <summary>
        /// 
        /// </summary>
        public IRuntimeObject? Me { get; private set; }

        /// <inheritdoc/>
        public object? this[string key]
        {
            get
            {
                if (key == "this") return Me;
                else if (Local.ContainsKey(key)) return Local[key];
                else if (Core.ContainsKey(key)) return Core[key];
                else if (Me is not null) return Me[key];
                else throw new KeyNotFoundException($"Property \"{key}\" was not found in the current context.");
            }
            set
            {
                Local[key] = value;
            }
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
        /// <param name="core"></param>
        /// <param name="local"></param>
        /// <param name="me"></param>
        private RuntimeContext(IDictionary<string, object?> core, IDictionary<string, object?> local, IRuntimeObject? me)
        {
            Core = core;
            Local = local;
            Me = me;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="me"></param>
        /// <param name="locals"></param>
        /// <returns></returns>
        public RuntimeContext SetSelf(IRuntimeObject me, params KeyValuePair<string, object?>[] locals) 
            => new RuntimeContext(Core, new Dictionary<string, object?>(locals), me);
    }
}
