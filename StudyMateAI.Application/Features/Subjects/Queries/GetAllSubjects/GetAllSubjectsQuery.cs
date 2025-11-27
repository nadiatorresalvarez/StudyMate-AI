using MediatR;
using StudyMateAI.Application.DTOs.Subject;

namespace StudyMateAI.Application.Features.Subjects.Queries.GetAllSubjects
{
    public class GetAllSubjectsQuery : IRequest<IEnumerable<SubjectResponseDto>>
    {
        public int UserId { get; set; }

        public GetAllSubjectsQuery(int userId)
        {
            UserId = userId;
        }
    }
}

