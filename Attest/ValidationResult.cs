namespace Attest
{
    /// <summary>
    /// The answer to one question about one value: whether it is valid, and if not, why. Every
    /// <c>Validate</c> method in this library returns one rather than throwing, including for null,
    /// for an empty string and for a value in the wrong script.
    /// </summary>
    public sealed class ValidationResult
    {

        /// <summary>A valid value.</summary>
        public static ValidationResult Success()
        {
            return new ValidationResult() { IsValid = true };
        }

        /// <summary>Invalid for a reason none of the other factories names.</summary>
        /// <param name="errorMessage">Why the value was rejected.</param>
        public static ValidationResult Invalid(string errorMessage)
        {
            return new ValidationResult() { ErrorMessage = errorMessage, IsValid = false };
        }

        /// <summary>
        /// Well formed, but its check digit does not match the rest of the number. This is the
        /// answer for a value that was mistyped rather than made up.
        /// </summary>
        public static ValidationResult InvalidChecksum()
        {
            return new ValidationResult() { ErrorMessage = $"Invalid checksum.", IsValid = false };
        }
        /// <summary>The wrong shape for this country's identifier.</summary>
        /// <param name="format">
        /// An example of the right shape, shown to the caller. It has to be a value this library
        /// would accept: a hint that contradicts the rule sends the caller looking for the wrong
        /// defect, which is how Brazil's digits-only example outlived the letters it started to
        /// accept in 2026.
        /// </param>
        public static ValidationResult InvalidFormat(string format)
        {
            return new ValidationResult() { ErrorMessage = $"Invalid format. The code must have this format {format}", IsValid = false };
        }
        /// <summary>Carries a date that does not exist, or one outside the range the country issues.</summary>
        public static ValidationResult InvalidDate()
        {
            return new ValidationResult() { ErrorMessage = $"Invalid date", IsValid = false };
        }

        /// <summary>The wrong number of characters.</summary>
        public static ValidationResult InvalidLength()
        {
            return new ValidationResult() { ErrorMessage = $"Invalid length", IsValid = false };
        }

        /// <summary>Whether the value satisfies the rule it was asked about.</summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Why the value was rejected, or null when it was not. Intended for a developer reading a
        /// log; it is not localised and not meant to be shown to the person whose number it is.
        /// </summary>
        public string ErrorMessage { get; private set; }
    }
}
