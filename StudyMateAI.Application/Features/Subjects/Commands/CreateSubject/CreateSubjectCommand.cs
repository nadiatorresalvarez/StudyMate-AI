using MediatR;
using StudyMateAI.Application.DTOs.Subject;

namespace StudyMateAI.Application.Features.Subjects.Commands.CreateSubject
{
    public class CreateSubjectCommand : IRequest<SubjectResponseDto>
    {
        public int UserId { get; set; }
        public CreateSubjectDto CreateDto { get; set; }

        public CreateSubjectCommand(int userId, CreateSubjectDto createDto)
        {
            UserId = userId;
            CreateDto = createDto;
        }
    }
}

