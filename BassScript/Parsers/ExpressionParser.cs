using BassClefStudio.BassScript.Data;
using BassClefStudio.BassScript.Runtime;
using Pidgin;
using Pidgin.Expression;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace BassClefStudio.BassScript.Parsers
{
    /// <summary>
    /// A service that builds and provides <see cref="Pidgin.Parser"/>s for parsing <see cref="IExpression"/>s from <see cref="string"/>-based content.
    /// </summary>
    public class ExpressionParser
    {
        private Parser<char, T> Token<T>(Parser<char, T> token)
            => Try(token).Before(SkipWhitespaces);
        private Parser<char, string> Token(string token)
            => Token(String(token));

        private Parser<char, T> Parenthesised<T>(Parser<char, T> parser)
            => parser.Between(Token("("), Token(")"));

        private Parser<char, Func<IExpression, IExpression, IExpression>> Binary(Parser<char, BinaryOperator> op)
            => op.Select<Func<IExpression, IExpression, IExpression>>(type => (l, r) => new BinaryOperation(l, r, type));
        private Parser<char, Func<IExpression, IExpression>> Unary(Parser<char, UnaryOperator> op)
            => op.Select<Func<IExpression, IExpression>>(type => o => new UnaryOperation(o, type));

        private Parser<char, Func<IExpression, IExpression, IExpression>> Add;
        private Parser<char, Func<IExpression, IExpression, IExpression>> Subtract;
        private Parser<char, Func<IExpression, IExpression, IExpression>> Multiply;
        private Parser<char, Func<IExpression, IExpression, IExpression>> Divide;

        private Parser<char, Func<IExpression, IExpression, IExpression>> Set;
        private Parser<char, Func<IExpression, IExpression, IExpression>> Define;
        private Parser<char, Func<IExpression, IExpression, IExpression>> Property;

        private Parser<char, Func<IExpression, IExpression, IExpression>> EqualTo;
        private Parser<char, Func<IExpression, IExpression, IExpression>> NotEqualTo;
        private Parser<char, Func<IExpression, IExpression, IExpression>> GThan;
        private Parser<char, Func<IExpression, IExpression, IExpression>> LThan;
        private Parser<char, Func<IExpression, IExpression, IExpression>> GThanEq;
        private Parser<char, Func<IExpression, IExpression, IExpression>> LThanEq;

        private Parser<char, Func<IExpression, IExpression>> Negative;
        private Parser<char, Func<IExpression, IExpression>> Not;

        private Parser<char, IExpression> Integer;
        private Parser<char, IExpression> Double;
        private Parser<char, IExpression> String;
        private Parser<char, IExpression> Boolean;
        private Parser<char, IExpression> Identifier;

        /// <summary>
        /// A <see cref="Parser{TToken, T}"/> which takes in text input and parses an <see cref="IExpression"/> AST.
        /// </summary>
        public Parser<char, IExpression> Expression { get; }

        /// <summary>
        /// Creates and initializes a <see cref="ExpressionParser"/> to parse SymbolicLanguage expressions.
        /// </summary>
        public ExpressionParser()
        {
            Integer = Token(Num)
                .Select<IExpression>(value => new IntegerExpression(value))
                .Labelled("int");

            var decimalParser =
                from neg in Char('-').Optional().Select(o => o.HasValue)
                from digits in Digit.AtLeastOnceString()
                from d in Char('.').Then(Digit.AtLeastOnceString())
                select double.Parse($"{(neg ? "-" : string.Empty)}{digits}.{d}");

            Double = Token(decimalParser)
                .Select<IExpression>(value => new DoubleExpression(value))
                .Labelled("double");

            var escapedString =
                OneOf(
                    AnyCharExcept('\"', '\\')
                        .Or(Char('\\').Then(Any))
                        .ManyString()
                        .Between(Char('\"')),
                    AnyCharExcept('\'', '\\')
                        .Or(Char('\\').Then(Any))
                        .ManyString()
                        .Between(Char('\'')));

            String = Token(escapedString)
                .Select<IExpression>(value => new StringExpression(value))
                .Labelled("string");

            var boolParser = String("true").ThenReturn(true)
                .Or(String("false").ThenReturn(false));

            Boolean = Token(boolParser)
                .Select<IExpression>(value => new BoolExpression(value))
                .Labelled("bool");

            Identifier = Token(Letter.Then(LetterOrDigit.ManyString(), (h, t) => h + t))
                .Select<IExpression>(name => new Identifier(name))
                .Labelled("identifier");

            Add = Binary(Token("+").ThenReturn(BinaryOperator.Add));
            Subtract = Binary(Token("-").ThenReturn(BinaryOperator.Subtract));
            Multiply = Binary(Token("*").ThenReturn(BinaryOperator.Multiply));
            Divide = Binary(Token("/").ThenReturn(BinaryOperator.Divide));

            Set = Binary(Token("=").ThenReturn(BinaryOperator.Set));
            Define = Binary(Token(":").ThenReturn(BinaryOperator.Define));
            Property = Binary(Token(".").ThenReturn(BinaryOperator.Property));

            EqualTo = Binary(Token("==").ThenReturn(BinaryOperator.EqualTo));
            NotEqualTo = Binary(Token("!=").ThenReturn(BinaryOperator.NotEqualTo));
            GThan = Binary(Token(">").ThenReturn(BinaryOperator.GThan));
            LThan = Binary(Token("<").ThenReturn(BinaryOperator.LThan));
            GThanEq = Binary(Token(">=").ThenReturn(BinaryOperator.GThanEq));
            LThanEq = Binary(Token("<=").ThenReturn(BinaryOperator.LThanEq));

            Negative = Unary(Token("-").ThenReturn(UnaryOperator.Negative));
            Not = Unary(Token("!").ThenReturn(UnaryOperator.Not));

            Parser<char, Func<IExpression, IExpression>> Call(Parser<char, IExpression> subExpr)
                => Parenthesised(subExpr.Separated(Token(",")))
                    .Select<Func<IExpression, IExpression>>(args => method => new FunctionCall(method, args))
                    .Labelled("function");

            Parser<char, Func<IExpression, IExpression>> Equate()
                => Try(Parenthesised(Identifier.Separated(Token(",")))
                    .Or(Identifier.Select<IEnumerable<IExpression>>(i => new[] {i})).Before(Token(":=")))
                    .Select<Func<IExpression, IExpression>>(
                        args => method => new BinaryOperation(new LambdaInputs(args), method, BinaryOperator.Lambda))
                    .Labelled("lambda expression");
            
            Expression = Pidgin.Expression.ExpressionParser.Build<char, IExpression>(
                expr => (
                    OneOf(
                        Identifier,
                        Boolean,
                        String,
                        Try(Double),
                        Integer,
                        Parenthesised(expr).Labelled("parenthetical")
                    ),
                    new OperatorTableRow<char, IExpression>[]
                    {
                        Operator.InfixL(Property),
                        Operator.PostfixChainable(Call(expr)),
                        Operator.Prefix(Negative),
                        Operator.Prefix(Not),
                        Operator.InfixL(Multiply)
                            .And(Operator.InfixL(Divide)),
                        Operator.InfixL(Add)
                            .And(Operator.InfixL(Subtract)),
                        Operator.InfixN(EqualTo)
                            .And(Operator.InfixN(NotEqualTo))
                            .And(Operator.InfixN(GThan))
                            .And(Operator.InfixN(GThanEq))
                            .And(Operator.InfixN(LThan))
                            .And(Operator.InfixN(LThanEq)),
                        Operator.PrefixChainable(Equate()),
                        Operator.InfixN(Define)
                            .And(Operator.InfixN(Set))
                    })).Labelled("expression");
        }

        /// <inheritdoc/>
        public IExpression BuildExpression(string input)
        {
            return Expression.ParseOrThrow(input);
        }
    }

    /// <summary>
    /// An <see cref="Exception"/> thrown when an expression could not be parsed into its resulting <see cref="IExpression"/> equivalent.
    /// </summary>
    [Serializable]
    public class CompileException : Exception
    {
        /// <inheritdoc/>
        public CompileException() { }

        /// <inheritdoc/>
        public CompileException(string message) : base(message) { }

        /// <inheritdoc/>
        public CompileException(string message, Exception inner) : base(message, inner) { }
        
        /// <inheritdoc/>
        protected CompileException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
