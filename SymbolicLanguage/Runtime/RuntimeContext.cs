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
        /// Contains the collection of <see cref="IRuntimeObject"/> layers used to resolve values in the <see cref="IRuntimeContext"/>.
        /// </summary>
        protected Stack<IRuntimeObject> Stack { get; }

        /// <inheritdoc/>
        public object? this[string key]
        {
            get
            {
                foreach (var data in Stack)
                {
                    try
                    {
                        return data[key];
                    }
                    catch (KeyNotFoundException)
                    { }
                }
                throw new KeyNotFoundException($"Could not find \"{key}\" in the current context.");
            }
            set => Stack.Peek()[key] = value;
        }

        /// <summary>
        /// Creates a new empty <see cref="RuntimeContext"/>.
        /// </summary>
        public RuntimeContext()
        {
            Stack = new Stack<IRuntimeObject>();
        }

        /// <summary>
        /// Creates a new <see cref="RuntimeContext"/> from a collection of <see cref="IRuntimeObject"/>s.
        /// </summary>
        /// <param name="stack">The <see cref="IRuntimeObject"/> context layers of the current <see cref="IRuntimeContext"/>.</param>
        public RuntimeContext(IEnumerable<IRuntimeObject> stack)
        {
            Stack = new Stack<IRuntimeObject>(stack);
        }

        /// <summary>
        /// Pops the most recent data from the <see cref="RuntimeContext"/>.
        /// </summary>
        /// <returns>The resulting <see cref="RuntimeContext"/>.</returns>
        public RuntimeContext PopContext()
        {
            return new RuntimeContext(Stack.Take(Stack.Count - 1));
        }

        /// <summary>
        /// Pushes some <see cref="IRuntimeObject"/> data to the <see cref="RuntimeContext"/>.
        /// </summary>
        /// <returns>The resulting <see cref="RuntimeContext"/>.</returns>
        public RuntimeContext PushContext(IRuntimeObject data)
        {
            if (data is RuntimeContext context)
            {
                return new RuntimeContext(Stack.Concat(context.Stack));
            }
            else
            {
                return new RuntimeContext(Stack.Concat(new[] { data }));
            }
        }
    }
}
