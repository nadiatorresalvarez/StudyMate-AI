using MediatR;
using StudyMateAI.Application.DTOs.Document;

namespace StudyMateAI.Application.Features.Documents.Queries.GetDocumentsBySubject
{
    public class GetDocumentsBySubjectQuery : IRequest<IEnumerable<DocumentResponseDto>>
    {
        public int UserId { get; set; }
        public int SubjectId { get; set; }

        public GetDocumentsBySubjectQuery(int userId, int subjectId)
        {
            UserId = userId;
            SubjectId = subjectId;
        }
    }
}

