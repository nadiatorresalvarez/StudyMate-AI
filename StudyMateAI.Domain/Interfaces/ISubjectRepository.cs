using StudyMateAI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyMateAI.Domain.Interfaces
{
    /// <summary>
    /// Repositorio específico para Subject con operaciones adicionales
    /// </summary>
    public interface ISubjectRepository : IRepository<Subject>
    {
        Task<IEnumerable<Subject>> GetByUserIdAsync(int userId);
        Task<Subject?> GetByIdAndUserIdAsync(int id, int userId);
        Task<bool> HasDocumentsAsync(int subjectId);
        Task<IEnumerable<Subject>> GetActiveByUserIdAsync(int userId);
    }
}
