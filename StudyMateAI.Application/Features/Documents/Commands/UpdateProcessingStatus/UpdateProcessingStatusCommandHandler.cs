using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs.Document;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Documents.Commands.UpdateProcessingStatus
{
    public class UpdateProcessingStatusCommandHandler : IRequestHandler<UpdateProcessingStatusCommand, DocumentResponseDto?>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public UpdateProcessingStatusCommandHandler(
            IDocumentRepository documentRepository,
            ISubjectRepository subjectRepository,
            IMapper mapper)
        {
            _documentRepository = documentRepository;
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<DocumentResponseDto?> Handle(UpdateProcessingStatusCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el documento pertenece al usuario
            var userOwnsDocument = await _documentRepository.UserOwnsDocumentAsync(request.DocumentId, request.UserId);
            if (!userOwnsDocument)
                return null;

            var document = await _documentRepository.GetByIdAsync(request.DocumentId);
            if (document == null)
                return null;

            document.ProcessingStatus = request.StatusDto.ProcessingStatus;
            document.ProcessingError = request.StatusDto.ProcessingError;
            
            if (request.StatusDto.ProcessedAt.HasValue)
                document.ProcessedAt = request.StatusDto.ProcessedAt;
            else if (request.StatusDto.ProcessingStatus == "Completed")
                document.ProcessedAt = DateTime.UtcNow;

            await _documentRepository.UpdateAsync(document);

            var subject = await _subjectRepository.GetByIdAsync(document.SubjectId);
            
            var response = _mapper.Map<DocumentResponseDto>(document);
            response.SubjectName = subject?.Name;
            response.FlashcardCount = document.Flashcards?.Count ?? 0;
            response.QuizCount = document.Quizzes?.Count ?? 0;
            response.SummaryCount = document.Summaries?.Count ?? 0;
            response.MindmapCount = document.Mindmaps?.Count ?? 0;
            response.ConceptmapCount = document.Conceptmaps?.Count ?? 0;

            return response;
        }
    }
}

