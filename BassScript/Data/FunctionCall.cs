namespace BassClefStudio.BassScript.Data
{
    /// <summary>
    /// An <see cref="IExpression"/> which calls another <see cref="IExpression"/> with a given set of <see cref="IExpression"/> inputs.
    /// </summary>
    public class FunctionCall : IExpression
    {
        /// <summary>
        /// The function or other <see cref="IExpression"/> that this <see cref="FunctionCall"/> calls.
        /// </summary>
        public IExpression Expression { get; }

        /// <summary>
        /// An ordered collection of <see cref="IExpression"/> inputs to the function.
        /// </summary>
        public IEnumerable<IExpression> Inputs { get; }

        /// <summary>
        /// Creates a new <see cref="FunctionCall"/>.
        /// </summary>
        /// <param name="expression">The function or other <see cref="IExpression"/> that this <see cref="FunctionCall"/> calls.</param>
        /// <param name="inputs">An ordered collection of <see cref="IExpression"/> inputs to the function.</param>
        public FunctionCall(IExpression expression, params IExpression[] inputs) : this(expression, inputs as IEnumerable<IExpression>)
        { }

        /// <summary>
        /// Creates a new <see cref="FunctionCall"/>.
        /// </summary>
        /// <param name="expression">The function or other <see cref="IExpression"/> that this <see cref="FunctionCall"/> calls.</param>
        /// <param name="inputs">An ordered collection of <see cref="IExpression"/> inputs to the function.</param>
        public FunctionCall(IExpression expression, IEnumerable<IExpression> inputs)
        {
            Expression = expression;
            Inputs = inputs;
        }

        /// <inheritdoc/>
        public bool Equals(IExpression? other)
        {
            return other is FunctionCall call
                && this.Expression.Equals(call.Expression)
                && this.Inputs.SequenceEqual(call.Inputs);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"({Expression}{string.Concat(Inputs.Select(i => " " + i))})";
        }
    }

    /// <summary>
    /// An <see cref="IExpression"/> which contains a collection of named inputs to a lambda expression.
    /// </summary>
    public class LambdaInputs : IExpression
    {
        /// <summary>
        /// A collection of the <see cref="Identifier"/>s used for the inputs to the lambda expression.
        /// </summary>
        public Identifier[] InputNames { get; }

        /// <summary>
        /// Creates a new <see cref="LambdaInputs"/>.
        /// </summary>
        /// <param name="inputs">A collection of the <see cref="Identifier"/>s used for the inputs to the lambda expression.</param>
        public LambdaInputs(params Identifier[] inputs)
        {
            InputNames = inputs;
        }

        /// <summary>
        /// Creates a new <see cref="LambdaInputs"/>.
        /// </summary>
        /// <param name="inputs">A collection of the <see cref="Identifier"/>s used for the inputs to the lambda expression.</param>
        public LambdaInputs(IEnumerable<Identifier> inputs)
        {
            InputNames = inputs.ToArray();
        }
        
        /// <summary>
        /// Creates a new <see cref="LambdaInputs"/>.
        /// </summary>
        /// <param name="inputs">A collection of the <see cref="IExpression"/>s used for the inputs to the lambda expression.</param>
        public LambdaInputs(IEnumerable<IExpression> inputs)
        {
            InputNames = inputs.OfType<Identifier>().ToArray();
        }

        /// <inheritdoc/>
        public bool Equals(IExpression? other)
        {
            return other is LambdaInputs ins
                   && this.InputNames.Equals(ins.InputNames);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"({string.Join<Identifier>(",", InputNames)})";
        }
    }
}
