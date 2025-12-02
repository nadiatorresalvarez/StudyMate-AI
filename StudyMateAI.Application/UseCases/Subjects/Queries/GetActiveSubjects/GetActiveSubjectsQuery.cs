using MediatR;
using StudyMateAI.Application.DTOs.Subject;

namespace StudyMateAI.Application.Features.Subjects.Queries.GetActiveSubjects
{
    public class GetActiveSubjectsQuery : IRequest<IEnumerable<SubjectResponseDto>>
    {
        public int UserId { get; set; }

        public GetActiveSubjectsQuery(int userId)
        {
            UserId = userId;
        }
    }
}

