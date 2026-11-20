namespace Capitra.Application.Common.Exceptions;

public sealed class BusinessValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;

    public static BusinessValidationException For(string field, string error) =>
        new(new Dictionary<string, string[]> { [field] = [error] });
}
