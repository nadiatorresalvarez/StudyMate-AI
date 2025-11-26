using MediatR;
using StudyMateAI.Application.DTOs.Document;

namespace StudyMateAI.Application.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentByIdQuery : IRequest<DocumentResponseDto?>
    {
        public int UserId { get; set; }
        public int DocumentId { get; set; }

        public GetDocumentByIdQuery(int userId, int documentId)
        {
            UserId = userId;
            DocumentId = documentId;
        }
    }
}

