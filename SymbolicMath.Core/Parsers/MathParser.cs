using BassClefStudio.SymbolicMath.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using Pidgin;
using Pidgin.Expression;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace BassClefStudio.SymbolicMath.Core.Parsers
{
    /// <summary>
    /// A service that builds and provides <see cref="Pidgin.Parser"/>s for parsing <see cref="IExpression"/>s from <see cref="string"/>-based content.
    /// </summary>
    public class MathParser : IExpressionBuilder<string>
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
        private Parser<char, Func<IExpression, IExpression, IExpression>> Equate;

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

        private Parser<char, Func<IExpression, IExpression>> Call(Parser<char, IExpression> subExpr)
            => Parenthesised(subExpr.Separated(Token(",")))
                .Select<Func<IExpression, IExpression>>(args => method => new FunctionCall(method, args))
                .Labelled("function");

        private Parser<char, IExpression> Expression;

        /// <summary>
        /// Creates and initializes a <see cref="MathParser"/> to parse mathematical expressions.
        /// </summary>
        public MathParser()
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
                AnyCharExcept('\"', '\\')
                .Or(Char('\\').Then(Any))
                .ManyString()
                .Between(Char('\"'));

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
            Equate = Binary(Token("=>").ThenReturn(BinaryOperator.Equate));

            EqualTo = Binary(Token("==").ThenReturn(BinaryOperator.EqualTo));
            NotEqualTo = Binary(Token("!=").ThenReturn(BinaryOperator.NotEqualTo));
            GThan = Binary(Token(">").ThenReturn(BinaryOperator.GThan));
            LThan = Binary(Token("<").ThenReturn(BinaryOperator.LThan));
            GThanEq = Binary(Token(">=").ThenReturn(BinaryOperator.GThanEq));
            LThanEq = Binary(Token("<=").ThenReturn(BinaryOperator.LThanEq));

            Negative = Unary(Token("-").ThenReturn(UnaryOperator.Negative));
            Not = Unary(Token("!").ThenReturn(UnaryOperator.Not));

            Expression = ExpressionParser.Build<char, IExpression>(expr => ( 
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
                    Operator.InfixN(Set)
                })).Labelled("expression");
        }

        /// <inheritdoc/>
        public IExpression BuildExpression(string input)
        {
            return Expression.ParseOrThrow(input);
        }
    }
}
