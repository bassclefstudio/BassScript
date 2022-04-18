using BassClefStudio.BassScript.Data;
using CommunityToolkit.Diagnostics;

namespace BassClefStudio.BassScript.Runtime
{
    /// <summary>
    /// A basic implementation of <see cref="IExpressionRuntime"/> which can execute <see cref="IExpression"/> expressions.
    /// </summary>
    public class ExpressionRuntime : IExpressionRuntime
    {
        #region Execution

        /// <inheritdoc/>
        public async Task<object?> ExecuteAsync(IExpression expression, IRuntimeObject context)
        {
            return expression switch
            {
                BinaryOperation binary => await ExecuteAsync(binary, context),
                UnaryOperation unary => await ExecuteAsync(unary, context),
                FunctionCall function => await ExecuteAsync(function, context),
                Identifier identifier => context[identifier.Name],
                ILiteral literal => literal.GetValue(),
                _ => throw new RuntimeException(expression, "Could not handle the provided kind of expression.")
            };
        }

        #region Sub-Evaluations

        /// <summary>
        /// Executes a <see cref="RuntimeMethod"/> encapsulated by the provided <see cref="IExpression"/>.
        /// </summary>
        /// <param name="method">The <see cref="FunctionCall"/> call to the <see cref="RuntimeMethod"/> method.</param>
        /// <param name="context">The <see cref="IRuntimeObject"/> context for evaluating <see cref="IExpression"/>s.</param>
        /// <returns>The <see cref="object"/> result of the method call.</returns>
        /// <exception cref="RuntimeException">The <see cref="RuntimeMethod"/> could not be located, or failed to execute.</exception>
        private async Task<object?> ExecuteAsync(FunctionCall method, IRuntimeObject context)
        {
            IExpression[] inputs = method.Inputs.ToArray();
            object? methodObject = await ExecuteAsync(method.Expression, context);
            if (methodObject is RuntimeMethod runtimeMethod)
            {
                object?[] inputObjects = new object?[inputs.Length];
                for (int i = 0; i < inputs.Length; i++)
                {
                    inputObjects[i] = await ExecuteAsync(inputs[i], context);
                }

                return await runtimeMethod(inputObjects);
            }
            else
            {
                throw new RuntimeException(method, "Cannot call an expression which is not a method.");
            }
        }

        /// <summary>
        /// Evaluates a <see cref="BinaryOperation"/>.
        /// </summary>
        /// <param name="binary">The <see cref="BinaryOperation"/> to evaluate.</param>
        /// <param name="context">The <see cref="IRuntimeObject"/> context for evaluating <see cref="IExpression"/>s.</param>
        /// <returns>The <see cref="object"/> result of the operation.</returns>
        /// <exception cref="RuntimeException">The operation was not supported by the current <see cref="IExpressionRuntime"/>.</exception>
        private async Task<object?> ExecuteAsync(BinaryOperation binary, IRuntimeObject context)
        {
            if (binary.Operator == BinaryOperator.Property)
            {
                object? left = await ExecuteAsync(binary.ArgA, context);
                if (left is null)
                {
                    throw new RuntimeException(binary, "Null reference exception: Cannot find property of null value.");
                }
                else
                {
                    if (left is IRuntimeObject runtimeObject)
                    {
                        return await ExecuteAsync(binary.ArgB, runtimeObject);
                    }
                    else
                    {
                        throw new RuntimeException(binary, "Cannot find property of a struct or literal type.");
                    }
                }
            }
            else if (binary.Operator == BinaryOperator.Set)
            {
                if (binary.ArgA is Identifier id)
                {
                    object? right = await ExecuteAsync(binary.ArgB, context);
                    return new VarBinding(async data => data[id.Name] = right);
                }
                else
                {
                    throw new RuntimeException(binary, $"Cannot create a variable binding for the variable {{{binary.ArgA}}}.");
                }
            }
            else if (binary.Operator == BinaryOperator.Equate)
            {
                if (binary.ArgA is Identifier id)
                {
                    object? right = await ExecuteAsync(binary.ArgB, context);
                    if (right is RuntimeMethod method)
                    {
                        return new VarBinding(data => data[id.Name] = method);
                    }
                    else
                    {
                        return new VarBinding(data => data[id.Name] = new RuntimeMethod(async inputs => await ExecuteAsync(binary.ArgB, data)));
                    }
                }
                else
                {
                    throw new RuntimeException(binary, $"Cannot create a variable binding for the variable {{{binary.ArgA}}}.");
                }
            }
            else
            {
                object? left = await ExecuteAsync(binary.ArgA, context);
                object? right = await ExecuteAsync(binary.ArgB, context);

                if (left is int li && right is int ri)
                {
                    return binary.Operator switch
                    {
                        BinaryOperator.EqualTo => li == ri,
                        BinaryOperator.NotEqualTo => li != ri,
                        BinaryOperator.Add => li + ri,
                        BinaryOperator.Subtract => li - ri,
                        BinaryOperator.Multiply => li * ri,
                        BinaryOperator.Divide => li / ri,
                        BinaryOperator.GThan => li > ri,
                        BinaryOperator.GThanEq => li >= ri,
                        BinaryOperator.LThan => li < ri,
                        BinaryOperator.LThanEq => li <= ri,
                        _ => throw new RuntimeException(binary, $"Operator type not supported: {binary.Operator}.")
                    };
                }
                else if (left is double ld && right is double rd)
                {
                    return binary.Operator switch
                    {
                        BinaryOperator.EqualTo => ld == rd,
                        BinaryOperator.NotEqualTo => ld != rd,
                        BinaryOperator.Add => ld + rd,
                        BinaryOperator.Subtract => ld - rd,
                        BinaryOperator.Multiply => ld * rd,
                        BinaryOperator.Divide => ld / rd,
                        BinaryOperator.GThan => ld > rd,
                        BinaryOperator.GThanEq => ld >= rd,
                        BinaryOperator.LThan => ld < rd,
                        BinaryOperator.LThanEq => ld <= rd,
                        _ => throw new RuntimeException(binary, $"Operator type not supported: {binary.Operator}.")
                    };
                }
                else if (left is bool lb && right is bool rb)
                {
                    return binary.Operator switch
                    {
                        BinaryOperator.EqualTo => lb == rb,
                        BinaryOperator.NotEqualTo => lb != rb,
                        BinaryOperator.And => lb && rb,
                        BinaryOperator.Or => lb || rb,
                        _ => throw new RuntimeException(binary, $"Operator type not supported: {binary.Operator}.")
                    };
                }
                else if (left is string ls && right is string rs)
                {
                    return binary.Operator switch
                    {
                        BinaryOperator.EqualTo => ls == rs,
                        BinaryOperator.NotEqualTo => ls != rs,
                        BinaryOperator.Add => ls + rs,
                        BinaryOperator.GThan => ls.CompareTo(rs) > 0,
                        BinaryOperator.GThanEq => ls.CompareTo(rs) >= 0,
                        BinaryOperator.LThan => ls.CompareTo(rs) < 0,
                        BinaryOperator.LThanEq => ls.CompareTo(rs) <= 0,
                        _ => throw new RuntimeException(binary, $"Operator type not supported: {binary.Operator}.")
                    };
                }
                else if (left is string lc && right is int rc)
                {
                    return binary.Operator switch
                    {
                        BinaryOperator.Multiply => string.Concat(Enumerable.Repeat(lc, rc)),
                        _ => throw new RuntimeException(binary, $"Operator type not supported: {binary.Operator}.")
                    };
                }
                else
                {
                    return binary.Operator switch
                    {
                        BinaryOperator.EqualTo => object.Equals(left, right),
                        BinaryOperator.NotEqualTo => !object.Equals(left, right),
                        _ => throw new RuntimeException(binary, $"Operator type not supported: {binary.Operator}.")
                    };
                }
            }
        }

        /// <summary>
        /// Evaluates an <see cref="UnaryOperation"/>.
        /// </summary>
        /// <param name="unary">The <see cref="UnaryOperation"/> to evaluate.</param>
        /// <param name="context">The <see cref="IRuntimeObject"/> context for evaluating <see cref="IExpression"/>s.</param>
        /// <returns>The <see cref="object"/> result of the operation.</returns>
        /// <exception cref="RuntimeException">The operation was not supported by the current <see cref="IExpressionRuntime"/>.</exception>
        private async Task<object?> ExecuteAsync(UnaryOperation unary, IRuntimeObject context)
        {
            object? parameter = await ExecuteAsync(unary.Arg, context);
            if (parameter is int i)
            {
                return unary.Operator switch
                {
                    UnaryOperator.Negative => -i,
                    _ => throw new RuntimeException(unary, $"Operator type not supported: {unary.Operator}.")
                };
            }
            else if (parameter is double d)
            {
                return unary.Operator switch
                {
                    UnaryOperator.Negative => -d,
                    _ => throw new RuntimeException(unary, $"Operator type not supported: {unary.Operator}.")
                };
            }
            else if (parameter is bool b)
            {
                return unary.Operator switch
                {
                    UnaryOperator.Not => !b,
                    _ => throw new RuntimeException(unary, $"Operator type not supported: {unary.Operator}.")
                };
            }
            else
            {
                return unary.Operator switch
                {
                    _ => throw new RuntimeException(unary, $"Operator type not supported: {unary.Operator}.")
                };
            }
        }

        #endregion
        #endregion
        #region Default Methods

        /// <summary>
        /// Creates a <see cref="RuntimeMethod"/> for the given <see cref="IRuntimeObject"/> which sets the given variable binding.
        /// </summary>
        /// <param name="me">The <see cref="IRuntimeObject"/> used as context ('this') for the binding operation.</param>
        /// <returns>The resulting <see cref="RuntimeMethod"/>.</returns>
        /// <exception cref="ArgumentException">An <see cref="Exception"/> thrown if the input to the <see cref="RuntimeMethod"/> is not a valid <see cref="VarBinding"/>.</exception>
        public static RuntimeMethod Let(IRuntimeObject me)
        {
            return async inputs => {
                Guard.HasSizeEqualTo(inputs, 1, nameof(inputs));
                object? letBinding = inputs[0];
                Guard.IsNotNull(letBinding, nameof(inputs));
                if (letBinding is VarBinding binding)
                {
                    binding(me);
                    return null;
                }
                else
                {
                    throw new ArgumentException("Cannot use let binding on an object that is not a VarBinding.");
                }
            };
        }
        
        /// <summary>
        /// Creates a <see cref="RuntimeMethod"/> which runs a LINQ Select expression.
        /// </summary>
        /// <returns>The resulting <see cref="RuntimeMethod"/>.</returns>
        /// <exception cref="ArgumentException">An <see cref="Exception"/> thrown if the inputs to the <see cref="RuntimeMethod"/> are not of the valid type.</exception>
        public static RuntimeMethod Select()
        {
            return async inputs => {
                Guard.HasSizeEqualTo(inputs, 2, nameof(inputs));
                object? collection = inputs[0];
                Guard.IsNotNull(collection, nameof(inputs));
                if (collection is IEnumerable<object?> items)
                {
                    object? condition = inputs[1];
                    Guard.IsNotNull(condition, nameof(inputs));
                    if (condition is RuntimeMethod method)
                    {
                        return await Task.WhenAll(items.Select(async s => await method(new[] { s })));
                    }
                    else
                    {
                        throw new ArgumentException("Select requires a collection and a Func<bool> as inputs.");
                    }
                }
                else
                {
                    throw new ArgumentException("Select requires a collection and a Func<bool> as inputs.");
                }
            };
        }

        private static async Task<T> As<T>(RuntimeMethod method, object?[] inputs)
        {
            object? result = await method(inputs);
            if (result is T tResult)
            {
                return tResult;
            }
            else
            {
                throw new InvalidCastException($"Could not cast the result of a method as {typeof(T)} - returned {result} instead.");
            }
        }
        
        #endregion
    }
}
