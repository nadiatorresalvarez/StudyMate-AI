using MediatR;

namespace StudyMateAI.Application.Features.Subjects.Queries.CanDeleteSubject
{
    public class CanDeleteSubjectQuery : IRequest<bool>
    {
        public int SubjectId { get; set; }

        public CanDeleteSubjectQuery(int subjectId)
        {
            SubjectId = subjectId;
        }
    }
}

