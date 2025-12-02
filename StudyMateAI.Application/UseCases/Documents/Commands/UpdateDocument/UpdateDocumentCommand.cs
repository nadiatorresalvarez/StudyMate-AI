using MediatR;
using StudyMateAI.Application.DTOs.Document;

namespace StudyMateAI.Application.Features.Documents.Commands.UpdateDocument
{
    public class UpdateDocumentCommand : IRequest<DocumentResponseDto?>
    {
        public int UserId { get; set; }
        public int DocumentId { get; set; }
        public UpdateDocumentDto UpdateDto { get; set; }

        public UpdateDocumentCommand(int userId, int documentId, UpdateDocumentDto updateDto)
        {
            UserId = userId;
            DocumentId = documentId;
            UpdateDto = updateDto;
        }
    }
}

