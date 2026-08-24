namespace MeterImportV2.Service
{
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string Message { get; } = string.Empty;
        protected ValidationResult() { }
        protected ValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }

        public static ValidationResult Success()
        {
            return new ValidationResult(true, string.Empty);
        }
        public static ValidationResult Fail(string message)
        {
            return new ValidationResult(false, message);
        }
    }
}
