using BassClefStudio.SymbolicMath.Core.Expressions;
using BassClefStudio.SymbolicMath.Core.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pidgin;
using System;
using System.Linq;

namespace BassClefStudio.SymbolicMath.Tests
{
    [TestClass]
    public class MathParseTests
    {
        public static MathParser MathParser;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            MathParser = new MathParser();
        }

        #region Basic Parsing

        private void TestBinary(string token, BinaryOperator op)
        {
            var ex1 = MathParser.BuildExpression($"x{token}y");
            var ex2 = MathParser.BuildExpression($"x {token} y");
            Console.WriteLine(ex1);

            var ex3 = MathParser.BuildExpression($"42{token}365");
            var ex4 = MathParser.BuildExpression($"42 {token} 365");
            Console.WriteLine(ex4);

            Assert.IsInstanceOfType(ex1, typeof(BinaryOperation), "Statement returned incorrect type.");
            Assert.IsInstanceOfType(ex2, typeof(BinaryOperation), "Statement returned incorrect type.");
            Assert.IsInstanceOfType(ex3, typeof(BinaryOperation), "Statement returned incorrect type.");
            Assert.IsInstanceOfType(ex4, typeof(BinaryOperation), "Statement returned incorrect type.");
            Assert.AreEqual(op, (ex1 as BinaryOperation).Operator, "Incorrect operator parsed.");
            Assert.IsTrue(ex1.Equals(ex2), "Whitespace caused incorrect parsing.");
            Assert.IsTrue(ex3.Equals(ex4), "Whitespace caused incorrect parsing.");
            Assert.ThrowsException<ParseException>(() => MathParser.BuildExpression($"132{token}"));
        }

        [TestMethod]
        public void ParseAddition() => TestBinary("+", BinaryOperator.Add);

        [TestMethod]
        public void ParseSubtraction() => TestBinary("-", BinaryOperator.Subtract);

        [TestMethod]
        public void ParseMultiplication() => TestBinary("*", BinaryOperator.Multiply);

        [TestMethod]
        public void ParseDivision() => TestBinary("/", BinaryOperator.Divide);

        [TestMethod]
        public void ParseNegative()
        {
            var expression = MathParser.BuildExpression("-426");
            Console.WriteLine(expression);
            Assert.IsInstanceOfType(expression, typeof(UnaryOperation), "Statement returned incorrect type.");
            UnaryOperation op = expression as UnaryOperation;
            Assert.IsInstanceOfType(op.Arg, typeof(Integer), "Argument of negative operation was not expected Integer.");
            Assert.AreEqual(426, (op.Arg as Integer).Value, "Input value to negative operation was not the provided value.");
        }

        private void AssociativeOperators(string token, BinaryOperator op)
        {
            var ex1 = MathParser.BuildExpression($"x{token}y{token}z");
            Assert.IsInstanceOfType(ex1, typeof(BinaryOperation), "Statement returned incorrect type.");
            BinaryOperation bin1 = ex1 as BinaryOperation;
            // Left associative operators...
            Assert.IsInstanceOfType(bin1.ArgA, typeof(BinaryOperation), "Statement returned incorrect type.");
            BinaryOperation bin2 = bin1.ArgA as BinaryOperation;
            Assert.AreEqual(op, bin1.Operator, "Incorrect operator parsed.");
            Assert.AreEqual(op, bin2.Operator, "Incorrect operator parsed.");

            Assert.IsInstanceOfType(bin2.ArgA, typeof(Identifier), "Operand was parsed incorrectly.");
            Assert.IsInstanceOfType(bin2.ArgB, typeof(Identifier), "Operand was parsed incorrectly.");
            Assert.IsInstanceOfType(bin1.ArgB, typeof(Identifier), "Operand was parsed incorrectly.");
        }

        [TestMethod]
        public void AssociativeAddition() => AssociativeOperators("+", BinaryOperator.Add);

        [TestMethod]
        public void AssociativeSubtraction() => AssociativeOperators("-", BinaryOperator.Subtract);

        [TestMethod]
        public void AssociativeMultiplication() => AssociativeOperators("*", BinaryOperator.Multiply);

        [TestMethod]
        public void AssociativeDivision() => AssociativeOperators("/", BinaryOperator.Divide);

        [TestMethod]
        public void ParseParenthetical()
        {
            var ex1 = MathParser.BuildExpression("(524)");
            Console.WriteLine(ex1);
            var ex2 = MathParser.BuildExpression("(524 + y)");
            Console.WriteLine(ex2);
            Assert.IsInstanceOfType(ex1, typeof(Integer), "Statement returned incorrect type.");
            Assert.IsInstanceOfType(ex2, typeof(BinaryOperation), "Statement returned incorrect type.");
            Assert.AreEqual(BinaryOperator.Add, (ex2 as BinaryOperation).Operator, "Incorrect operator parsed.");
        }

        [TestMethod]
        public void ParseCall()
        {
            var ex1 = MathParser.BuildExpression("f(524)");
            Console.WriteLine(ex1);
            var ex2 = MathParser.BuildExpression("f(524, y)");
            Console.WriteLine(ex2);
            Assert.IsInstanceOfType(ex1, typeof(FunctionCall), "Statement returned incorrect type.");
            Assert.IsInstanceOfType(ex2, typeof(FunctionCall), "Statement returned incorrect type.");
            Assert.AreEqual(1, (ex1 as FunctionCall).Inputs.Count(), "Incorrect number of function inputs.");
            Assert.AreEqual(2, (ex2 as FunctionCall).Inputs.Count(), "Incorrect number of function inputs.");
        }

        #endregion
        #region Order of Operations



        #endregion
    }
}
