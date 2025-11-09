using StudyMateAI.Application.DTOs.Subject;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyMateAI.Application.Services
{
    public interface ISubjectService
    {
        Task<SubjectResponseDto> CreateSubjectAsync(int userId, CreateSubjectDto createDto);
        Task<SubjectResponseDto?> GetSubjectByIdAsync(int userId, int subjectId);
        Task<IEnumerable<SubjectResponseDto>> GetAllSubjectsAsync(int userId);
        Task<IEnumerable<SubjectResponseDto>> GetActiveSubjectsAsync(int userId);
        Task<SubjectResponseDto?> UpdateSubjectAsync(int userId, int subjectId, UpdateSubjectDto updateDto);
        Task<bool> DeleteSubjectAsync(int userId, int subjectId);
        Task<bool> CanDeleteSubjectAsync(int subjectId);
    }
}
