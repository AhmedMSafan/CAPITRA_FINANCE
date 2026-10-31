namespace Capitra.Application.Common.Crud;

/// <summary>A create/update body. <see cref="RowVersion"/> is the version the user loaded, required on update.</summary>
public interface ISaveRequest
{
    int? RowVersion { get; }
}
