using StudyMateAI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyMateAI.Domain.Interfaces
{
    /// <summary>
    /// Repositorio específico para Document con operaciones adicionales
    /// </summary>
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<IEnumerable<Document>> GetBySubjectIdAsync(int subjectId);
        Task<Document?> GetByIdAndSubjectIdAsync(int id, int subjectId);
        Task<IEnumerable<Document>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Document>> GetByProcessingStatusAsync(int userId, string status);
        Task<bool> UserOwnsDocumentAsync(int documentId, int userId);
    }
}

