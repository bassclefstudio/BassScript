using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.SymbolicLanguage.Data
{
    /// <summary>
    /// Represents a basic token of any type.
    /// </summary>
    public interface IExpression : IEquatable<IExpression>
    { }
}