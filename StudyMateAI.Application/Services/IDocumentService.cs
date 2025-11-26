using StudyMateAI.Application.DTOs.Document;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyMateAI.Application.Services
{
    public interface IDocumentService
    {
        Task<DocumentResponseDto> CreateDocumentAsync(int userId, CreateDocumentDto createDto);
        Task<DocumentResponseDto?> GetDocumentByIdAsync(int userId, int documentId);
        Task<IEnumerable<DocumentResponseDto>> GetAllDocumentsAsync(int userId);
        Task<IEnumerable<DocumentResponseDto>> GetDocumentsBySubjectAsync(int userId, int subjectId);
        Task<IEnumerable<DocumentResponseDto>> GetDocumentsByStatusAsync(int userId, string status);
        Task<DocumentResponseDto?> UpdateDocumentAsync(int userId, int documentId, UpdateDocumentDto updateDto);
        Task<DocumentResponseDto?> UpdateProcessingStatusAsync(int userId, int documentId, UpdateProcessingStatusDto statusDto);
        Task<bool> DeleteDocumentAsync(int userId, int documentId);
        Task<bool> UserOwnsDocumentAsync(int userId, int documentId);
    }
}

