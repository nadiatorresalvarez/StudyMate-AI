using AutoMapper;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Application.DTOs.Document;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudyMateAI.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IFileStorage _fileStorage;
        private readonly IMapper _mapper;

        public DocumentService(
            IDocumentRepository documentRepository,
            ISubjectRepository subjectRepository,
            IFileStorage fileStorage,
            IMapper mapper)
        {
            _documentRepository = documentRepository;
            _subjectRepository = subjectRepository;
            _fileStorage = fileStorage;
            _mapper = mapper;
        }

        public async Task<DocumentResponseDto> CreateDocumentAsync(int userId, CreateDocumentDto createDto)
        {
            // Verificar que la materia existe y pertenece al usuario
            var subject = await _subjectRepository.GetByIdAndUserIdAsync(createDto.SubjectId, userId);
            if (subject == null)
            {
                throw new UnauthorizedAccessException("La materia no existe o no pertenece al usuario");
            }

            var document = _mapper.Map<Document>(createDto);
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

        public async Task<DocumentResponseDto?> GetDocumentByIdAsync(int userId, int documentId)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            
            if (document == null)
                return null;

            // Verificar que el documento pertenece al usuario
            var userOwns = await _documentRepository.UserOwnsDocumentAsync(documentId, userId);
            if (!userOwns)
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

        public async Task<IEnumerable<DocumentResponseDto>> GetAllDocumentsAsync(int userId)
        {
            var documents = await _documentRepository.GetByUserIdAsync(userId);
            
            return documents.Select(d =>
            {
                var dto = _mapper.Map<DocumentResponseDto>(d);
                dto.SubjectName = d.Subject?.Name;
                dto.FlashcardCount = d.Flashcards?.Count ?? 0;
                dto.QuizCount = d.Quizzes?.Count ?? 0;
                dto.SummaryCount = d.Summaries?.Count ?? 0;
                dto.MindmapCount = d.Mindmaps?.Count ?? 0;
                dto.ConceptmapCount = d.Conceptmaps?.Count ?? 0;
                return dto;
            });
        }

        public async Task<IEnumerable<DocumentResponseDto>> GetDocumentsBySubjectAsync(int userId, int subjectId)
        {
            // Verificar que la materia pertenece al usuario
            var subject = await _subjectRepository.GetByIdAndUserIdAsync(subjectId, userId);
            if (subject == null)
            {
                return Enumerable.Empty<DocumentResponseDto>();
            }

            var documents = await _documentRepository.GetBySubjectIdAsync(subjectId);
            
            return documents.Select(d =>
            {
                var dto = _mapper.Map<DocumentResponseDto>(d);
                dto.SubjectName = d.Subject?.Name;
                dto.FlashcardCount = d.Flashcards?.Count ?? 0;
                dto.QuizCount = d.Quizzes?.Count ?? 0;
                dto.SummaryCount = d.Summaries?.Count ?? 0;
                dto.MindmapCount = d.Mindmaps?.Count ?? 0;
                dto.ConceptmapCount = d.Conceptmaps?.Count ?? 0;
                return dto;
            });
        }

        public async Task<IEnumerable<DocumentResponseDto>> GetDocumentsByStatusAsync(int userId, string status)
        {
            var documents = await _documentRepository.GetByProcessingStatusAsync(userId, status);
            
            return documents.Select(d =>
            {
                var dto = _mapper.Map<DocumentResponseDto>(d);
                dto.SubjectName = d.Subject?.Name;
                dto.FlashcardCount = d.Flashcards?.Count ?? 0;
                dto.QuizCount = d.Quizzes?.Count ?? 0;
                dto.SummaryCount = d.Summaries?.Count ?? 0;
                dto.MindmapCount = d.Mindmaps?.Count ?? 0;
                dto.ConceptmapCount = d.Conceptmaps?.Count ?? 0;
                return dto;
            });
        }

        public async Task<DocumentResponseDto?> UpdateDocumentAsync(int userId, int documentId, UpdateDocumentDto updateDto)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            
            if (document == null)
                return null;

            // Verificar que el documento pertenece al usuario
            var userOwns = await _documentRepository.UserOwnsDocumentAsync(documentId, userId);
            if (!userOwns)
                return null;

            // Actualizar solo los campos proporcionados
            if (!string.IsNullOrEmpty(updateDto.FileName))
                document.FileName = updateDto.FileName;

            if (updateDto.ExtractedText != null)
                document.ExtractedText = updateDto.ExtractedText;

            if (updateDto.FileSizeKb.HasValue)
                document.FileSizeKb = updateDto.FileSizeKb.Value;

            if (updateDto.PageCount.HasValue)
                document.PageCount = updateDto.PageCount.Value;

            if (!string.IsNullOrEmpty(updateDto.Language))
                document.Language = updateDto.Language;

            if (!string.IsNullOrEmpty(updateDto.ProcessingStatus))
                document.ProcessingStatus = updateDto.ProcessingStatus;

            if (updateDto.ProcessingError != null)
                document.ProcessingError = updateDto.ProcessingError;

            var updatedDocument = await _documentRepository.UpdateAsync(document);
            
            var response = _mapper.Map<DocumentResponseDto>(updatedDocument);
            response.SubjectName = updatedDocument.Subject?.Name;
            response.FlashcardCount = updatedDocument.Flashcards?.Count ?? 0;
            response.QuizCount = updatedDocument.Quizzes?.Count ?? 0;
            response.SummaryCount = updatedDocument.Summaries?.Count ?? 0;
            response.MindmapCount = updatedDocument.Mindmaps?.Count ?? 0;
            response.ConceptmapCount = updatedDocument.Conceptmaps?.Count ?? 0;
            
            return response;
        }

        public async Task<DocumentResponseDto?> UpdateProcessingStatusAsync(int userId, int documentId, UpdateProcessingStatusDto statusDto)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            
            if (document == null)
                return null;

            // Verificar que el documento pertenece al usuario
            var userOwns = await _documentRepository.UserOwnsDocumentAsync(documentId, userId);
            if (!userOwns)
                return null;

            document.ProcessingStatus = statusDto.ProcessingStatus;
            document.ProcessingError = statusDto.ProcessingError;
            document.ProcessedAt = statusDto.ProcessedAt ?? DateTime.UtcNow;

            var updatedDocument = await _documentRepository.UpdateAsync(document);
            
            var response = _mapper.Map<DocumentResponseDto>(updatedDocument);
            response.SubjectName = updatedDocument.Subject?.Name;
            response.FlashcardCount = updatedDocument.Flashcards?.Count ?? 0;
            response.QuizCount = updatedDocument.Quizzes?.Count ?? 0;
            response.SummaryCount = updatedDocument.Summaries?.Count ?? 0;
            response.MindmapCount = updatedDocument.Mindmaps?.Count ?? 0;
            response.ConceptmapCount = updatedDocument.Conceptmaps?.Count ?? 0;
            
            return response;
        }

        public async Task<bool> DeleteDocumentAsync(int userId, int documentId)
        {
            // Cargar documento
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                return false;

            // Verificar que el documento pertenece al usuario
            var userOwns = await _documentRepository.UserOwnsDocumentAsync(documentId, userId);
            if (!userOwns)
                return false;

            // Borrar archivo físico asociado (si existe)
            if (!string.IsNullOrWhiteSpace(document.FileUrl))
            {
                await _fileStorage.DeleteAsync(document.FileUrl, CancellationToken.None);
            }

            // Eliminar registro en BD (cascada configurada manejará entidades relacionadas)
            return await _documentRepository.DeleteAsync(documentId);
        }

        public async Task<bool> UserOwnsDocumentAsync(int userId, int documentId)
        {
            return await _documentRepository.UserOwnsDocumentAsync(documentId, userId);
        }
    }
}

