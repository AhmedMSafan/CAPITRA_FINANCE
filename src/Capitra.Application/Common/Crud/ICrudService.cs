namespace Capitra.Application.Common.Crud;

public interface ICrudService<TDto, in TSave>
    where TSave : ISaveRequest
{
    Task<TDto> GetAsync(long id, CancellationToken cancellationToken);

    Task<TDto> CreateAsync(TSave request, CancellationToken cancellationToken);

    Task<TDto> UpdateAsync(long id, TSave request, CancellationToken cancellationToken);

    Task DeleteAsync(long id, int rowVersion, CancellationToken cancellationToken);
}
