using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs.Subject;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Subjects.Commands.CreateSubject
{
    public class CreateSubjectCommandHandler : IRequestHandler<CreateSubjectCommand, SubjectResponseDto>
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public CreateSubjectCommandHandler(ISubjectRepository subjectRepository, IMapper mapper)
        {
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<SubjectResponseDto> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
        {
            var subject = _mapper.Map<Subject>(request.CreateDto);
            subject.UserId = request.UserId;
            subject.CreatedAt = DateTime.UtcNow;
            subject.IsArchived = false;

            var createdSubject = await _subjectRepository.AddAsync(subject);
            
            var response = _mapper.Map<SubjectResponseDto>(createdSubject);
            response.DocumentCount = 0;
            
            return response;
        }
    }
}

