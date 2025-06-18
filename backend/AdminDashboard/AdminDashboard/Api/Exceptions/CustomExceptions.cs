namespace Api.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName, object key)
            : base($"Entity '{entityName}' with key '{key}' was not found.") { }
    }

    public class EntityValidationException : Exception
    {
        public Dictionary<string, string> Errors { get; }

        public EntityValidationException(Dictionary<string, string> errors)
            : base("Validation failed for one or more entities.")
        {
            Errors = errors;
        }
    }

    public class EntityConflictException : Exception
    {
        public EntityConflictException(string entityName, string propertyName, object propertyValue)
            : base($"Entity '{entityName}' with {propertyName} '{propertyValue}' already exists.") { }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}