using System.Net.Mail;
using Capitra.Application.Common.Exceptions;

namespace Capitra.Application.Common.Crud;

/// <summary>Collects field errors while a request is checked, then throws them together as one 400 response.</summary>
public sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = [];

    public bool HasErrors => _errors.Count > 0;

    public void Add(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var list))
            _errors[field] = list = [];
        list.Add(message);
    }

    /// <summary>Trims the value, treats blank as null, and checks required and maximum length.</summary>
    public string? Text(string field, string label, string? value, int maxLength, bool required = false)
    {
        var text = string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        if (text is null && required)
            Add(field, $"{label} is required.");
        else if (text?.Length > maxLength)
            Add(field, $"{label} is limited to {maxLength} characters.");

        return text;
    }

    public string? Email(string field, string label, string? value, int maxLength)
    {
        var text = Text(field, label, value, maxLength);
        if (text is not null && !MailAddress.TryCreate(text, out _))
            Add(field, $"{label} is not a valid email address.");
        return text;
    }

    public void Range(string field, string label, decimal? value, decimal min, decimal max)
    {
        if (value is { } v && (v < min || v > max))
            Add(field, $"{label} must be between {min} and {max}.");
    }

    public void ThrowIfAny()
    {
        if (HasErrors)
            throw new BusinessValidationException(_errors.ToDictionary(x => x.Key, x => x.Value.ToArray()));
    }
}
