using System.Linq.Expressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Common.Crud;

/// <summary>
/// List, get, create, update and delete for a table with a bigint key, with optimistic concurrency on ROW_VERSION.
/// Each module service supplies the validation rules (from the APEX form), the field mapping and the list projection.
/// </summary>
public abstract class CrudService<TEntity, TDto, TSave>(ICapitraDbContext db) : ICrudService<TDto, TSave>
    where TEntity : AuditableEntity, new()
    where TSave : ISaveRequest
{
    protected ICapitraDbContext Db { get; } = db;

    /// <summary>Name of one record in messages, e.g. "Developer".</summary>
    protected abstract string Title { get; }

    protected abstract long IdOf(TEntity entity);

    protected abstract Expression<Func<TEntity, bool>> HasId(long id);

    /// <summary>Turns rows into DTOs, joining whatever names the screen shows (developer name, unit type name...).</summary>
    protected abstract IQueryable<TDto> Project(IQueryable<TEntity> rows);

    protected abstract Task ValidateAsync(TSave request, TEntity? existing, ValidationErrors errors, CancellationToken cancellationToken);

    protected abstract void Apply(TEntity entity, TSave request);

    protected async Task<IReadOnlyList<TDto>> ListAsync(Expression<Func<TEntity, bool>>? filter, CancellationToken cancellationToken)
    {
        var rows = Db.Set<TEntity>().AsNoTracking();
        if (filter is not null)
            rows = rows.Where(filter);
        return await Project(rows).ToListAsync(cancellationToken);
    }

    public async Task<TDto> GetAsync(long id, CancellationToken cancellationToken) =>
        await Project(Db.Set<TEntity>().AsNoTracking().Where(HasId(id))).FirstOrDefaultAsync(cancellationToken)
        ?? throw NotFound(id);

    public async Task<TDto> CreateAsync(TSave request, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        await ValidateAsync(request, existing: null, errors, cancellationToken);
        errors.ThrowIfAny();

        var entity = new TEntity();
        Apply(entity, request);
        Db.Add(entity);
        await Db.SaveChangesAsync(cancellationToken);

        return await GetAsync(IdOf(entity), cancellationToken);
    }

    public async Task<TDto> UpdateAsync(long id, TSave request, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);

        var errors = new ValidationErrors();
        if (request.RowVersion is null)
            errors.Add(nameof(request.RowVersion), "RowVersion is required when updating.");
        await ValidateAsync(request, entity, errors, cancellationToken);
        errors.ThrowIfAny();

        if (entity.RowVersion != request.RowVersion)
            throw ChangedByAnotherUser(id);

        Apply(entity, request);
        await SaveAsync(id, cancellationToken);

        return await GetAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(long id, int rowVersion, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity.RowVersion != rowVersion)
            throw ChangedByAnotherUser(id);

        Db.Remove(entity);
        await SaveAsync(id, cancellationToken);
    }

    /// <summary>True when another row already uses this value in a unique column.</summary>
    protected Task<bool> IsTakenAsync(Expression<Func<TEntity, bool>> sameValue, TEntity? existing, CancellationToken cancellationToken)
    {
        var rows = Db.Set<TEntity>().Where(sameValue);
        if (existing is not null)
            rows = rows.Where(Not(HasId(IdOf(existing))));
        return rows.AnyAsync(cancellationToken);
    }

    /// <summary>True when the referenced row exists and is active, or is the value the record already has.</summary>
    protected static async Task<bool> IsSelectableAsync<TRef>(
        IQueryable<TRef> activeMatches, bool unchanged, CancellationToken cancellationToken) =>
        unchanged || await activeMatches.AnyAsync(cancellationToken);

    private async Task<TEntity> FindAsync(long id, CancellationToken cancellationToken) =>
        await Db.Set<TEntity>().FirstOrDefaultAsync(HasId(id), cancellationToken) ?? throw NotFound(id);

    private async Task SaveAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            await Db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ChangedByAnotherUser(id);
        }
    }

    private static Expression<Func<TEntity, bool>> Not(Expression<Func<TEntity, bool>> predicate) =>
        Expression.Lambda<Func<TEntity, bool>>(Expression.Not(predicate.Body), predicate.Parameters);

    private NotFoundException NotFound(long id) => new($"{Title} {id} was not found.");

    private ConflictException ChangedByAnotherUser(long id) =>
        new($"{Title} {id} was changed by another user. Reload and try again.");
}
