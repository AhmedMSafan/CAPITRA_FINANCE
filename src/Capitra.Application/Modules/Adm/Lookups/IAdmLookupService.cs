namespace Capitra.Application.Modules.Adm.Lookups;

public interface IAdmLookupService
{
    IReadOnlyCollection<string> LookupNames { get; }

    Task<IReadOnlyList<LookupDto>> ListAsync(string lookupName, bool includeInactive, CancellationToken cancellationToken);

    Task<LookupDto> GetAsync(string lookupName, string code, CancellationToken cancellationToken);

    Task<LookupDto> CreateAsync(string lookupName, SaveLookupRequest request, CancellationToken cancellationToken);

    Task<LookupDto> UpdateAsync(string lookupName, string code, SaveLookupRequest request, CancellationToken cancellationToken);
}
