using MediatR;
using StudyMateAI.Application.DTOs.Subject;

namespace StudyMateAI.Application.Features.Subjects.Queries.GetSubjectById
{
    public class GetSubjectByIdQuery : IRequest<SubjectResponseDto?>
    {
        public int UserId { get; set; }
        public int SubjectId { get; set; }

        public GetSubjectByIdQuery(int userId, int subjectId)
        {
            UserId = userId;
            SubjectId = subjectId;
        }
    }
}

