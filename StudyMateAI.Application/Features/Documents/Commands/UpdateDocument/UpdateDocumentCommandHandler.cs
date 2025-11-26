using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs.Document;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Documents.Commands.UpdateDocument
{
    public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, DocumentResponseDto?>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public UpdateDocumentCommandHandler(
            IDocumentRepository documentRepository,
            ISubjectRepository subjectRepository,
            IMapper mapper)
        {
            _documentRepository = documentRepository;
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<DocumentResponseDto?> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el documento pertenece al usuario
            var userOwnsDocument = await _documentRepository.UserOwnsDocumentAsync(request.DocumentId, request.UserId);
            if (!userOwnsDocument)
                return null;

            var document = await _documentRepository.GetByIdAsync(request.DocumentId);
            if (document == null)
                return null;

            // Actualizar propiedades si se proporcionan
            if (!string.IsNullOrEmpty(request.UpdateDto.FileName))
                document.FileName = request.UpdateDto.FileName;

            if (request.UpdateDto.ExtractedText != null)
                document.ExtractedText = request.UpdateDto.ExtractedText;

            if (request.UpdateDto.FileSizeKb.HasValue)
                document.FileSizeKb = request.UpdateDto.FileSizeKb;

            if (request.UpdateDto.PageCount.HasValue)
                document.PageCount = request.UpdateDto.PageCount;

            if (!string.IsNullOrEmpty(request.UpdateDto.Language))
                document.Language = request.UpdateDto.Language;

            if (!string.IsNullOrEmpty(request.UpdateDto.ProcessingStatus))
                document.ProcessingStatus = request.UpdateDto.ProcessingStatus;

            if (request.UpdateDto.ProcessingError != null)
                document.ProcessingError = request.UpdateDto.ProcessingError;

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

