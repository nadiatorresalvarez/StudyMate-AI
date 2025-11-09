using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly dbContextStudyMateAI _context;

    public UnitOfWork(dbContextStudyMateAI context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}