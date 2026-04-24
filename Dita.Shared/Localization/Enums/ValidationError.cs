namespace Dita.Shared.Localization.Enums;

/// <summary>
/// User input and data validation-related error codes (range 7000-7999).
/// </summary>
public enum ValidationError
{
   /// <summary>
   /// Data type conversion failed.
   /// </summary>
   ConversionFailed = 7001,

   /// <summary>
   /// Duplicate value detected where uniqueness is required.
   /// </summary>
   DuplicateValue = 7002,

   /// <summary>
   /// Email address format is invalid.
   /// </summary>
   InvalidEmailFormat = 7003,

   /// <summary>
   /// Date or time format is invalid.
   /// </summary>
   InvalidDateTimeFormat = 7004,

   /// <summary>
   /// Input data format is invalid or unrecognized.
   /// </summary>
   InvalidFormat = 7005,

   /// <summary>
   /// JSON format is invalid or malformed.
   /// </summary>
   InvalidJsonFormat = 7006,

   /// <summary>
   /// Numeric format is invalid.
   /// </summary>
   InvalidNumericFormat = 7007,

   /// <summary>
   /// Phone number format is invalid.
   /// </summary>
   InvalidPhoneFormat = 7008,

   /// <summary>
   /// Regular expression pattern validation failed.
   /// </summary>
   InvalidPattern = 7009,

   /// <summary>
   /// URL format is invalid.
   /// </summary>
   InvalidUrlFormat = 7010,

   /// <summary>
   /// XML format is invalid or malformed.
   /// </summary>
   InvalidXmlFormat = 7011,

   /// <summary>
   /// Value exceeds maximum allowed length.
   /// </summary>
   MaxLengthExceeded = 7012,

   /// <summary>
   /// Value exceeds maximum allowed numeric value.
   /// </summary>
   MaxValueExceeded = 7013,

   /// <summary>
   /// Value is below minimum required length.
   /// </summary>
   MinLengthNotMet = 7014,

   /// <summary>
   /// Value is below minimum allowed numeric value.
   /// </summary>
   MinValueNotMet = 7015,

   /// <summary>
   /// Required field or value is missing.
   /// </summary>
   MissingRequiredField = 7016,

   /// <summary>
   /// No error occurred (success).
   /// </summary>
   None = 7000,

   /// <summary>
   /// Value is outside the allowed numeric range.
   /// </summary>
   OutOfRange = 7017,

   /// <summary>
   /// Password does not meet complexity requirements.
   /// </summary>
   PasswordComplexityNotMet = 7018,

   /// <summary>
   /// Unknown validation error occurred.
   /// </summary>
   UnknownValidationError = 7019,

   /// <summary>
   /// Value type is unsupported for the operation.
   /// </summary>
   UnsupportedValueType = 7020
}
