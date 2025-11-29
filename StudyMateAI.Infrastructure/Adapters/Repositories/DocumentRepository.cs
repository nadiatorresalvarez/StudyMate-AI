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
    /// Implementación específica del repositorio de Document
    /// </summary>
    public class DocumentRepository : Repository<Document>, IDocumentRepository
    {
        public DocumentRepository(dbContextStudyMateAI context) : base(context)
        {
        }

        public async Task<IEnumerable<Document>> GetBySubjectIdAsync(int subjectId)
        {
            return await _dbSet
                .Include(d => d.Subject)
                .Where(d => d.SubjectId == subjectId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<Document?> GetByIdAndSubjectIdAsync(int id, int subjectId)
        {
            return await _dbSet
                .Include(d => d.Subject)
                .Include(d => d.Flashcards)
                .Include(d => d.Quizzes)
                .Include(d => d.Summaries)
                .Include(d => d.Mindmaps)
                .Include(d => d.Conceptmaps)
                .FirstOrDefaultAsync(d => d.Id == id && d.SubjectId == subjectId);
        }

        public async Task<IEnumerable<Document>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(d => d.Subject)
                .Where(d => d.Subject.UserId == userId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByProcessingStatusAsync(int userId, string status)
        {
            return await _dbSet
                .Include(d => d.Subject)
                .Where(d => d.Subject.UserId == userId && d.ProcessingStatus == status)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<bool> UserOwnsDocumentAsync(int documentId, int userId)
        {
            return await _dbSet
                .AnyAsync(d => d.Id == documentId && d.Subject.UserId == userId);
        }

        public async Task<int> CountByUserAsync(int userId, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(d => d.Subject)
                .Where(d => d.Subject.UserId == userId)
                .CountAsync(ct);
        }
    }
}

