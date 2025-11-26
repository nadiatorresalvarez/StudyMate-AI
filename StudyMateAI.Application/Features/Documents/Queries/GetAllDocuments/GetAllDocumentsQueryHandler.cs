using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs.Document;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Documents.Queries.GetAllDocuments
{
    public class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, IEnumerable<DocumentResponseDto>>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IMapper _mapper;

        public GetAllDocumentsQueryHandler(IDocumentRepository documentRepository, IMapper mapper)
        {
            _documentRepository = documentRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DocumentResponseDto>> Handle(GetAllDocumentsQuery request, CancellationToken cancellationToken)
        {
            var documents = await _documentRepository.GetByUserIdAsync(request.UserId);
            
            var response = documents.Select(d =>
            {
                var dto = _mapper.Map<DocumentResponseDto>(d);
                dto.SubjectName = d.Subject?.Name;
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

