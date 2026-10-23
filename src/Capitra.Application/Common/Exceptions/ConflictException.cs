namespace Capitra.Application.Common.Exceptions;

/// <summary>The record exists already, or was changed by someone else since it was read.</summary>
public sealed class ConflictException(string message) : Exception(message);
