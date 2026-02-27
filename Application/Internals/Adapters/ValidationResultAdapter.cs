namespace Application.Internals.Adapters;

public sealed class ValidationResultAdapter
{
    public string Code { get; }
    public string Message { get; }
    public string? Field { get; }
    //public IReadOnlyCollection<ValidationError> Validations { get; init; } = Array.Empty<ValidationError>();
    public ValidationResultAdapter(string code, string message, string? field)
    {
        Code = code;
        Message = message;
        Field = field;
    }
}