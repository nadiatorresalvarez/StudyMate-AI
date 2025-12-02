using MediatR;
using StudyMateAI.Application.DTOs.Document;

namespace StudyMateAI.Application.Features.Documents.Queries.GetDocumentsByStatus
{
    public class GetDocumentsByStatusQuery : IRequest<IEnumerable<DocumentResponseDto>>
    {
        public int UserId { get; set; }
        public string Status { get; set; }

        public GetDocumentsByStatusQuery(int userId, string status)
        {
            UserId = userId;
            Status = status;
        }
    }
}

