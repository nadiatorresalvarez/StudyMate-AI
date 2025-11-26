using MediatR;
using StudyMateAI.Application.DTOs.Subject;

namespace StudyMateAI.Application.Features.Subjects.Commands.UpdateSubject
{
    public class UpdateSubjectCommand : IRequest<SubjectResponseDto?>
    {
        public int UserId { get; set; }
        public int SubjectId { get; set; }
        public UpdateSubjectDto UpdateDto { get; set; }

        public UpdateSubjectCommand(int userId, int subjectId, UpdateSubjectDto updateDto)
        {
            UserId = userId;
            SubjectId = subjectId;
            UpdateDto = updateDto;
        }
    }
}

