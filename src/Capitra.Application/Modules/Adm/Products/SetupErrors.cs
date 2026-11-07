using Capitra.Application.Common.Exceptions;

namespace Capitra.Application.Modules.Adm.Products;

/// <summary>Collects the field errors of a product or charge setup form; field names are the request's property names.</summary>
internal sealed class SetupErrors
{
    private readonly Dictionary<string, string[]> errors = [];

    public void Add(string field, string message)
    {
        if (!errors.ContainsKey(field))
            errors[field] = [message];
    }

    /// <summary>Trims a required text and checks its length; returns the trimmed value (upper-cased for codes).</summary>
    public string Required(string field, string label, string? value, int maxLength, bool upper = false)
    {
        var result = value?.Trim() ?? string.Empty;
        if (result.Length == 0)
            Add(field, $"{label} is required.");
        else if (result.Length > maxLength)
            Add(field, $"{label} cannot exceed {maxLength} characters.");
        return upper ? result.ToUpperInvariant() : result;
    }

    public string? Optional(string field, string label, string? value, int maxLength, bool upper = false)
    {
        var result = value?.Trim();
        if (string.IsNullOrEmpty(result))
            return null;
        if (result.Length > maxLength)
            Add(field, $"{label} cannot exceed {maxLength} characters.");
        return upper ? result.ToUpperInvariant() : result;
    }

    public string OneOf(string field, string label, string? value, IReadOnlyCollection<string> allowed)
    {
        var result = (value ?? string.Empty).Trim().ToUpperInvariant();
        if (result.Length == 0)
            Add(field, $"{label} is required.");
        else if (!allowed.Contains(result))
            Add(field, $"{label} must be one of {string.Join(", ", allowed)}.");
        return result;
    }

    public void Percent(string field, string label, decimal? value)
    {
        if (value is < 0m or > 100m)
            Add(field, $"{label} must be between 0 and 100.");
    }

    public void NotNegative(string field, string label, decimal? value)
    {
        if (value < 0m)
            Add(field, $"{label} cannot be negative.");
    }

    public void Dates(string field, DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && to.Value < from.Value)
            Add(field, "Effective To cannot be before Effective From.");
    }

    public void ThrowIfAny()
    {
        if (errors.Count > 0)
            throw new BusinessValidationException(errors);
    }
}
