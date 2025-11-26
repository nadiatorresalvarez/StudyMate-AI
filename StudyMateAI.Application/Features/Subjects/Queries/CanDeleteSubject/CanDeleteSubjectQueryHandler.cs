using MediatR;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Subjects.Queries.CanDeleteSubject
{
    public class CanDeleteSubjectQueryHandler : IRequestHandler<CanDeleteSubjectQuery, bool>
    {
        private readonly ISubjectRepository _subjectRepository;

        public CanDeleteSubjectQueryHandler(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public async Task<bool> Handle(CanDeleteSubjectQuery request, CancellationToken cancellationToken)
        {
            return await _subjectRepository.HasDocumentsAsync(request.SubjectId);
        }
    }
}
