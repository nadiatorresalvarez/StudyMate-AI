using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs.Subject;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Subjects.Queries.GetActiveSubjects
{
    public class GetActiveSubjectsQueryHandler : IRequestHandler<GetActiveSubjectsQuery, IEnumerable<SubjectResponseDto>>
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public GetActiveSubjectsQueryHandler(ISubjectRepository subjectRepository, IMapper mapper)
        {
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SubjectResponseDto>> Handle(GetActiveSubjectsQuery request, CancellationToken cancellationToken)
        {
            var subjects = await _subjectRepository.GetActiveByUserIdAsync(request.UserId);
            
            var response = subjects.Select(s =>
            {
                var dto = _mapper.Map<SubjectResponseDto>(s);
                dto.DocumentCount = s.Documents?.Count ?? 0;
                return dto;
            }).ToList();

            return response;
        }
    }
}

