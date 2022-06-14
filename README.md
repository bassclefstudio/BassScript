# BassScript
BassScript is a semi-compiled scripting language whose parser and runtime is written entirely in .NET 6. It is designed to interoperate with .NET objects and provide a way for users to dynamically interact with existing .NET models for data and operations. The syntax is similar to (but not identical to) C#, with some changes to encourage more functional programming as well as remove additional complexity from what is ostensibly a simplistic scripting language.

## Notable Features
- Support for dealing with .NET objects (including `Dictionary<TKey, T>` objects as well as any object which implements `IRuntimeObject`).
- First-class data support (lambda functions using `:=`, functions can be passed as parameters and set to variables, etc.)
- `IExpression`s which are used in the runtime can be parsed from code or created/altered within .NET (allowing for programmatic generation/optimization of code).
- (incomplete) Error support including parsing errors through `Pidgin` with location and documentation info, and runtime errors which locate the trace of offending expressions.