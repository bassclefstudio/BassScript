using System.Collections;
using System.Reflection;
using BassClefStudio.BassScript.Data;
using CommunityToolkit.Diagnostics;
using static System.String;

namespace BassClefStudio.BassScript.Runtime
{
    /// <summary>
    /// A basic implementation of <see cref="IExpressionRuntime"/> which can execute <see cref="IExpression"/> expressions.
    /// </summary>
    public class ExpressionRuntime : IExpressionRuntime
    {
#region Execution

        /// <inheritdoc/>
        public async Task<object?> ExecuteAsync(IExpression expression, RuntimeContext context)
        {
            try
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
            catch (RuntimeException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RuntimeException(expression, "Executing the given expression threw an exception.", ex);
            }
        }

#region Sub-Evaluations

        /// <summary>
        /// Executes a <see cref="RuntimeMethod"/> encapsulated by the provided <see cref="IExpression"/>.
        /// </summary>
        /// <param name="method">The <see cref="FunctionCall"/> call to the <see cref="RuntimeMethod"/> method.</param>
        /// <param name="context">The <see cref="RuntimeContext"/> context for evaluating <see cref="IExpression"/>s.</param>
        /// <returns>The <see cref="object"/> result of the method call.</returns>
        /// <exception cref="RuntimeException">The <see cref="RuntimeMethod"/> could not be located, or failed to execute.</exception>
        private async Task<object?> ExecuteAsync(FunctionCall method, RuntimeContext context)
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

                return await runtimeMethod(context, inputObjects);
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
        /// <param name="context">The <see cref="RuntimeContext"/> context for evaluating <see cref="IExpression"/>s.</param>
        /// <returns>The <see cref="object"/> result of the operation.</returns>
        /// <exception cref="RuntimeException">The operation was not supported by the current <see cref="IExpressionRuntime"/>.</exception>
        private async Task<object?> ExecuteAsync(BinaryOperation binary, RuntimeContext context)
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
                        return await ExecuteAsync(binary.ArgB, context.SetSelf(runtimeObject));
                    }
                    else if (binary.ArgB is Identifier id)
                    {
                        if (left is IDictionary dict) return dict[id.Name];
                        else
                        {
                            Type leftType = left.GetType();
                            Console.WriteLine($"Info: Using reflection over type {leftType}.");
                            var property = leftType.GetProperty(id.Name);
                            if (property is not null) return property.GetValue(left);
                            var field = leftType.GetField(id.Name);
                            if (field is not null) return field.GetValue(left);
                        }
                    }
                    throw new RuntimeException(binary, $"Cannot find property of the current object.");
                }
            }
            else if (binary.Operator == BinaryOperator.Set)
            {
                if (binary.ArgA is Identifier id)
                {
                    context[id.Name] = await ExecuteAsync(binary.ArgB, context);
                    return null;
                }
                else
                {
                    throw new RuntimeException(
                        binary,
                        $"Cannot create a variable binding for the variable {binary.ArgA}.");
                }
            }
            else if (binary.Operator == BinaryOperator.Define)
            {
                if (binary.ArgA is Identifier id)
                {
                    object? value = await ExecuteAsync(binary.ArgB, context);
                    return new DefBinding(newContext => newContext[id.Name] = value);
                }
                else
                {
                    throw new RuntimeException(
                        binary,
                        $"Cannot create a defined binding for the variable {binary.ArgA}.");
                }
            }
            else if (binary.Operator == BinaryOperator.Lambda)
            {
                if (binary.ArgA is LambdaInputs ins)
                {
                    return new RuntimeMethod(
                        async (newContext, inputs) => await ExecuteAsync(
                            binary.ArgB,
                            newContext.Copy(
                                ins.InputNames.Zip(inputs).Select(
                                    i => new KeyValuePair<string, object?>(i.First.Name, i.Second))
                                    .ToArray())));
                }
                else
                {
                    throw new RuntimeException(
                        binary,
                        $"Cannot create a variable binding for the variable {{{binary.ArgA}}}.");
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
                        BinaryOperator.GThan => Compare(ls, rs, StringComparison.Ordinal) > 0,
                        BinaryOperator.GThanEq => Compare(ls, rs, StringComparison.Ordinal) >= 0,
                        BinaryOperator.LThan => Compare(ls, rs, StringComparison.Ordinal) < 0,
                        BinaryOperator.LThanEq => Compare(ls, rs, StringComparison.Ordinal) <= 0,
                        _ => throw new RuntimeException(binary, $"Operator type not supported: {binary.Operator}.")
                    };
                }
                else if (left is string lc && right is int rc)
                {
                    return binary.Operator switch
                    {
                        BinaryOperator.Multiply => Concat(Enumerable.Repeat(lc, rc)),
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
        /// <param name="context">The <see cref="RuntimeContext"/> context for evaluating <see cref="IExpression"/>s.</param>
        /// <returns>The <see cref="object"/> result of the operation.</returns>
        /// <exception cref="RuntimeException">The operation was not supported by the current <see cref="IExpressionRuntime"/>.</exception>
        private async Task<object?> ExecuteAsync(UnaryOperation unary, RuntimeContext context)
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
#region Extension Methods

        /// <summary>
        /// Creates a strongly-typed <see cref="RuntimeMethod"/> from the given function.
        /// </summary>
        /// <param name="method">A function that takes a <typeparamref name="T"/> input and returns an <see cref="object"/>.</param>
        /// <typeparam name="T">The type of the input to the <see cref="RuntimeMethod"/>.</typeparam>
        /// <returns>The resulting <see cref="RuntimeMethod"/>.</returns>
        public static RuntimeMethod MakeMethod<T>(Func<RuntimeContext, T, Task<object?>> method)
            => async (context, inputs) =>
            {
                Guard.HasSizeEqualTo(inputs, 1, nameof(inputs));
                object? input0 = inputs[0];
                Guard.IsNotNull(input0, nameof(inputs));
                Guard.IsAssignableToType<T>(input0, nameof(inputs));
                return await method(context, (T) input0);
            };

        /// <summary>
        /// Creates a strongly-typed <see cref="RuntimeMethod"/> from the given function.
        /// </summary>
        /// <param name="method">A function that takes a <typeparamref name="T1"/> and <typeparamref name="T2"/> input and returns an <see cref="object"/>.</param>
        /// <typeparam name="T1">The type of the first input to the <see cref="RuntimeMethod"/>.</typeparam>
        /// <typeparam name="T2">The type of the second input to the <see cref="RuntimeMethod"/>.</typeparam>
        /// <returns>The resulting <see cref="RuntimeMethod"/>.</returns>
        public static RuntimeMethod MakeMethod<T1, T2>(Func<RuntimeContext, T1, T2, Task<object?>> method)
            => async (context, inputs) =>
            {
                Guard.HasSizeEqualTo(inputs, 2, nameof(inputs));
                object? input0 = inputs[0];
                object? input1 = inputs[1];
                Guard.IsNotNull(input0, nameof(inputs));
                Guard.IsNotNull(input1, nameof(inputs));
                Guard.IsAssignableToType<T1>(input0, nameof(inputs));
                Guard.IsAssignableToType<T2>(input1, nameof(inputs));
                return await method(context, (T1) input0, (T2) input1);
            };
        
        /// <summary>
        /// Creates a strongly-typed <see cref="RuntimeMethod"/> from the given function.
        /// </summary>
        /// <param name="method">A function that takes a <typeparamref name="T1"/>, <typeparamref name="T2"/>, and <typeparamref name="T3"/> input and returns an <see cref="object"/>.</param>
        /// <typeparam name="T1">The type of the first input to the <see cref="RuntimeMethod"/>.</typeparam>
        /// <typeparam name="T2">The type of the second input to the <see cref="RuntimeMethod"/>.</typeparam>
        /// <typeparam name="T3">The type of the third input to the <see cref="RuntimeMethod"/>.</typeparam>
        /// <returns>The resulting <see cref="RuntimeMethod"/>.</returns>
        public static RuntimeMethod MakeMethod<T1, T2, T3>(Func<RuntimeContext, T1, T2, T3, Task<object?>> method)
            => async (context, inputs) =>
            {
                Guard.HasSizeEqualTo(inputs, 3, nameof(inputs));
                object? input0 = inputs[0];
                object? input1 = inputs[1];
                object? input2 = inputs[2];
                Guard.IsNotNull(input0, nameof(inputs));
                Guard.IsNotNull(input1, nameof(inputs));
                Guard.IsNotNull(input2, nameof(inputs));
                Guard.IsAssignableToType<T1>(input0, nameof(inputs));
                Guard.IsAssignableToType<T2>(input1, nameof(inputs));
                Guard.IsAssignableToType<T3>(input2, nameof(inputs));
                return await method(context, (T1) input0, (T2) input1, (T3) input2);
            };
        
#endregion
    }
}