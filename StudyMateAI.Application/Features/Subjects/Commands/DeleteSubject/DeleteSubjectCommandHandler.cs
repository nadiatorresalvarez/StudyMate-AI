using MediatR;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Subjects.Commands.DeleteSubject
{
    public class DeleteSubjectCommandHandler : IRequestHandler<DeleteSubjectCommand, bool>
    {
        private readonly ISubjectRepository _subjectRepository;

        public DeleteSubjectCommandHandler(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public async Task<bool> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
        {
            var subject = await _subjectRepository.GetByIdAndUserIdAsync(request.SubjectId, request.UserId);
            
            if (subject == null)
                return false;

            await _subjectRepository.DeleteAsync(subject.Id);
            return true;
        }
    }
}
