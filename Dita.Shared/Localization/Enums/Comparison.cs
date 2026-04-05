namespace Dita.Shared.Localization.Enums;

/// <summary>
/// Specifies the types of comparison operations that can be performed when evaluating values.
/// </summary>
/// <remarks>
/// Use this enumeration to indicate the desired comparison logic, such as equality, relational comparisons, or range
/// checks. The Between value requires two operands representing the lower and upper bounds. The Any value matches all
/// inputs without restriction.
/// </remarks>
public enum Comparison
{
   /// <summary>
   /// Specifies that two values are equal.
   /// </summary>
   Equal,

   /// <summary>
   /// Represents a comparison operation that determines whether one value is greater than another.
   /// </summary>
   Greater,

   /// <summary>
   /// Specifies that a value must be greater than or equal to a comparison value.
   /// </summary>
   GreaterOrEqual,

   /// <summary>
   /// Represents a value or condition that is less than a specified reference point.
   /// </summary>
   Less,

   /// <summary>
   /// Specifies that a value is less than or equal to a comparison target.
   /// </summary>
   LessOrEqual,

   /// <summary>
   /// Represents a value or range that lies between two specified bounds.
   /// </summary>
   Between,

   /// <summary>
   /// Represents an option or value that matches any input or condition.
   /// </summary>
   /// <remarks>
   /// Use this member to specify that all possible cases should be handled or matched, regardless of their specific
   /// value.
   /// </remarks>
   Any
}