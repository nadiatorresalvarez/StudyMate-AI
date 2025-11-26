using MediatR;
using StudyMateAI.Application.DTOs.Document;

namespace StudyMateAI.Application.Features.Documents.Queries.GetAllDocuments
{
    public class GetAllDocumentsQuery : IRequest<IEnumerable<DocumentResponseDto>>
    {
        public int UserId { get; set; }

        public GetAllDocumentsQuery(int userId)
        {
            UserId = userId;
        }
    }
}

