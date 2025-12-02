using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs.Document;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentResponseDto?>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IMapper _mapper;

        public GetDocumentByIdQueryHandler(IDocumentRepository documentRepository, IMapper mapper)
        {
            _documentRepository = documentRepository;
            _mapper = mapper;
        }

        public async Task<DocumentResponseDto?> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
        {
            // Verificar que el documento pertenece al usuario
            var userOwnsDocument = await _documentRepository.UserOwnsDocumentAsync(request.DocumentId, request.UserId);
            if (!userOwnsDocument)
                return null;

            var document = await _documentRepository.GetByIdAsync(request.DocumentId);
            if (document == null)
                return null;

            var response = _mapper.Map<DocumentResponseDto>(document);
            response.SubjectName = document.Subject?.Name;
            response.FlashcardCount = document.Flashcards?.Count ?? 0;
            response.QuizCount = document.Quizzes?.Count ?? 0;
            response.SummaryCount = document.Summaries?.Count ?? 0;
            response.MindmapCount = document.Mindmaps?.Count ?? 0;
            response.ConceptmapCount = document.Conceptmaps?.Count ?? 0;

            return response;
        }
    }
}

