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
        private Parser<char, Func<IExpression, IExpression>> Negative;

        private Parser<char, IExpression> Integer;
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
                .Select<IExpression>(value => new Integer(value))
                .Labelled("int");

            Identifier = Token(Letter.Then(LetterOrDigit.ManyString(), (h, t) => h + t))
                .Select<IExpression>(name => new Identifier(name))
                .Labelled("identifier");

            Add = Binary(Token("+").ThenReturn(BinaryOperator.Add));
            Subtract = Binary(Token("-").ThenReturn(BinaryOperator.Subtract));
            Multiply = Binary(Token("*").ThenReturn(BinaryOperator.Multiply));
            Divide = Binary(Token("/").ThenReturn(BinaryOperator.Divide));
            Negative = Unary(Token("-").ThenReturn(UnaryOperator.Negative));

            Expression = ExpressionParser.Build<char, IExpression>(expr => ( 
                OneOf(
                    Identifier,
                    Integer,
                    Parenthesised(expr).Labelled("parenthetical")
                ),
                new OperatorTableRow<char, IExpression>[]
                {
                    Operator.PostfixChainable(Call(expr)),
                    Operator.Prefix(Negative),
                    Operator.InfixL(Multiply).And(Operator.InfixL(Divide)),
                    Operator.InfixL(Add).And(Operator.InfixL(Subtract))
                })).Labelled("expression");
        }

        /// <inheritdoc/>
        public IExpression BuildExpression(string input)
        {
            return Expression.ParseOrThrow(input);
        }
    }
}
