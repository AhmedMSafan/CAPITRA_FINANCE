namespace Capitra.Application.Common.Exceptions;

/// <summary>The record exists already, is still in use, or was changed by someone else since it was read.</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
