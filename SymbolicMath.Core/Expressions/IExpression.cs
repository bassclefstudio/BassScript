using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.SymbolicMath.Core.Expressions
{
    /// <summary>
    /// Represents a basic token of any type.
    /// </summary>
    public interface IExpression : IEquatable<IExpression>
    { }
}