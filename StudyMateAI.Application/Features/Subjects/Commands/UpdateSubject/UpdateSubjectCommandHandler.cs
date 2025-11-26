using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs.Subject;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Subjects.Commands.UpdateSubject
{
    public class UpdateSubjectCommandHandler : IRequestHandler<UpdateSubjectCommand, SubjectResponseDto?>
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public UpdateSubjectCommandHandler(ISubjectRepository subjectRepository, IMapper mapper)
        {
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<SubjectResponseDto?> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
        {
            var subject = await _subjectRepository.GetByIdAndUserIdAsync(request.SubjectId, request.UserId);
            
            if (subject == null)
                return null;

            // Actualizar propiedades
            subject.Name = request.UpdateDto.Name;
            subject.Description = request.UpdateDto.Description;
            subject.Color = request.UpdateDto.Color;
            subject.Icon = request.UpdateDto.Icon;
            subject.OrderIndex = request.UpdateDto.OrderIndex;
            subject.IsArchived = request.UpdateDto.IsArchived;

            await _subjectRepository.UpdateAsync(subject);
            
            var response = _mapper.Map<SubjectResponseDto>(subject);
            response.DocumentCount = subject.Documents?.Count ?? 0;
            
            return response;
        }
    }
}
