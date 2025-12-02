using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs.Document;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, DocumentResponseDto>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public CreateDocumentCommandHandler(
            IDocumentRepository documentRepository,
            ISubjectRepository subjectRepository,
            IMapper mapper)
        {
            _documentRepository = documentRepository;
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<DocumentResponseDto> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
        {
            // Verificar que la materia pertenezca al usuario
            var subject = await _subjectRepository.GetByIdAndUserIdAsync(request.CreateDto.SubjectId, request.UserId);
            if (subject == null)
            {
                throw new UnauthorizedAccessException("La materia no existe o no pertenece al usuario");
            }

            var document = _mapper.Map<Document>(request.CreateDto);
            document.UploadedAt = DateTime.UtcNow;
            document.ProcessingStatus = "Pending";

            var createdDocument = await _documentRepository.AddAsync(document);

            var response = _mapper.Map<DocumentResponseDto>(createdDocument);
            response.SubjectName = subject.Name;
            response.FlashcardCount = 0;
            response.QuizCount = 0;
            response.SummaryCount = 0;
            response.MindmapCount = 0;
            response.ConceptmapCount = 0;

            return response;
        }
    }
}

