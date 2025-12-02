using MediatR;
using StudyMateAI.Application.DTOs.Document;

namespace StudyMateAI.Application.Features.Documents.Commands.UpdateProcessingStatus
{
    public class UpdateProcessingStatusCommand : IRequest<DocumentResponseDto?>
    {
        public int UserId { get; set; }
        public int DocumentId { get; set; }
        public UpdateProcessingStatusDto StatusDto { get; set; }

        public UpdateProcessingStatusCommand(int userId, int documentId, UpdateProcessingStatusDto statusDto)
        {
            UserId = userId;
            DocumentId = documentId;
            StatusDto = statusDto;
        }
    }
}

