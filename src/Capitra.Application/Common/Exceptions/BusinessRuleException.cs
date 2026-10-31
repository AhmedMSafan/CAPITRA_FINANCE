namespace Capitra.Application.Common.Exceptions;

/// <summary>A request the business rules do not allow at this point, e.g. completing a task that is already done.</summary>
public sealed class BusinessRuleException(string message) : Exception(message);
