using MediatR;
using StudyMateAI.Application.DTOs.Document;

namespace StudyMateAI.Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommand : IRequest<DocumentResponseDto>
    {
        public int UserId { get; set; }
        public CreateDocumentDto CreateDto { get; set; }

        public CreateDocumentCommand(int userId, CreateDocumentDto createDto)
        {
            UserId = userId;
            CreateDto = createDto;
        }
    }
}

