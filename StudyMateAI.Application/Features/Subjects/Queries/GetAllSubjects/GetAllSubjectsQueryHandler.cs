using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs.Subject;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Subjects.Queries.GetAllSubjects
{
    public class GetAllSubjectsQueryHandler : IRequestHandler<GetAllSubjectsQuery, IEnumerable<SubjectResponseDto>>
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public GetAllSubjectsQueryHandler(ISubjectRepository subjectRepository, IMapper mapper)
        {
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SubjectResponseDto>> Handle(GetAllSubjectsQuery request, CancellationToken cancellationToken)
        {
            var subjects = await _subjectRepository.GetByUserIdAsync(request.UserId);
            
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
