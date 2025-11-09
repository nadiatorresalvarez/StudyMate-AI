using Microsoft.EntityFrameworkCore;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;
using StudyMateAI.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudyMateAI.Infrastructure.Adapters.Repositories
{
    /// <summary>
    /// Implementación específica del repositorio de Subject
    /// </summary>
    public class SubjectRepository : Repository<Subject>, ISubjectRepository
    {
        public SubjectRepository(dbContextStudyMateAI context) : base(context)
        {
        }

        public async Task<IEnumerable<Subject>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.OrderIndex)
                .ThenBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Subject?> GetByIdAndUserIdAsync(int id, int userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        }

        public async Task<bool> HasDocumentsAsync(int subjectId)
        {
            return await _context.Documents
                .AnyAsync(d => d.SubjectId == subjectId);
        }

        public async Task<IEnumerable<Subject>> GetActiveByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(s => s.UserId == userId && (s.IsArchived == false || s.IsArchived == null))
                .OrderBy(s => s.OrderIndex)
                .ThenBy(s => s.Name)
                .ToListAsync();
        }
    }
}
