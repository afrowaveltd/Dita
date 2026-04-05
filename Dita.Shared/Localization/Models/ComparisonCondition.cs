using Dita.Shared.Localization.Enums;

namespace Dita.Shared.Localization.Models;

/// <summary>
/// Represents a condition that compares one or more integer values using a specified comparison operator and logical
/// combination.
/// </summary>
/// <remarks>
/// Use this class to define comparison-based conditions, such as filtering or matching scenarios, where multiple values
/// and logical operators (AND/OR) are required. The comparison operator and logical combination are configurable via
/// the properties.
/// </remarks>
public class ComparisonCondition
{
   /// <summary>
   /// Gets or sets the comparison operation to use when evaluating values.
   /// </summary>
   public Comparison Compare { get; set; } = Comparison.Equal;

   /// <summary>
   /// Gets or sets the collection of integer values associated with this instance.
   /// </summary>
   public int[] Values { get; set; } = [];

   /// <summary>
   /// Gets or sets a value indicating whether to combine multiple conditions using OR (true) or AND (false).
   /// </summary>
   public bool IsOr { get; set; } = false; //if true, we use OR (for example equal 1 OR eqaul 2), if false, we use AND (equal 1 AND equal 2)
}