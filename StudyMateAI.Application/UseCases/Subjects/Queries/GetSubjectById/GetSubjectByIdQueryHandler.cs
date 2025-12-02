using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs.Subject;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Subjects.Queries.GetSubjectById
{
    public class GetSubjectByIdQueryHandler : IRequestHandler<GetSubjectByIdQuery, SubjectResponseDto?>
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public GetSubjectByIdQueryHandler(ISubjectRepository subjectRepository, IMapper mapper)
        {
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<SubjectResponseDto?> Handle(GetSubjectByIdQuery request, CancellationToken cancellationToken)
        {
            var subject = await _subjectRepository.GetByIdAndUserIdAsync(request.SubjectId, request.UserId);
            
            if (subject == null)
                return null;

            var response = _mapper.Map<SubjectResponseDto>(subject);
            response.DocumentCount = subject.Documents?.Count ?? 0;
            
            return response;
        }
    }
}

