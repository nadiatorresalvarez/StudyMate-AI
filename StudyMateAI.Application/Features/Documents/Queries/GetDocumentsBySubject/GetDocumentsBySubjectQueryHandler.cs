using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs.Document;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Documents.Queries.GetDocumentsBySubject
{
    public class GetDocumentsBySubjectQueryHandler : IRequestHandler<GetDocumentsBySubjectQuery, IEnumerable<DocumentResponseDto>>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public GetDocumentsBySubjectQueryHandler(
            IDocumentRepository documentRepository,
            ISubjectRepository subjectRepository,
            IMapper mapper)
        {
            _documentRepository = documentRepository;
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DocumentResponseDto>> Handle(GetDocumentsBySubjectQuery request, CancellationToken cancellationToken)
        {
            // Verificar que la materia pertenece al usuario
            var subject = await _subjectRepository.GetByIdAndUserIdAsync(request.SubjectId, request.UserId);
            if (subject == null)
                return Enumerable.Empty<DocumentResponseDto>();

            var documents = await _documentRepository.GetBySubjectIdAsync(request.SubjectId);
            
            var response = documents.Select(d =>
            {
                var dto = _mapper.Map<DocumentResponseDto>(d);
                dto.SubjectName = subject.Name;
                dto.FlashcardCount = d.Flashcards?.Count ?? 0;
                dto.QuizCount = d.Quizzes?.Count ?? 0;
                dto.SummaryCount = d.Summaries?.Count ?? 0;
                dto.MindmapCount = d.Mindmaps?.Count ?? 0;
                dto.ConceptmapCount = d.Conceptmaps?.Count ?? 0;
                return dto;
            }).ToList();

            return response;
        }
    }
}

