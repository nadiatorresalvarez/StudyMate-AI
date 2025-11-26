using MediatR;

namespace StudyMateAI.Application.Features.Subjects.Commands.DeleteSubject
{
    public class DeleteSubjectCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public int SubjectId { get; set; }

        public DeleteSubjectCommand(int userId, int subjectId)
        {
            UserId = userId;
            SubjectId = subjectId;
        }
    }
}

