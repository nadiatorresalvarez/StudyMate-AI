using MediatR;
using StudyMateAI.Application.DTOs.Document;
using System.IO;

namespace StudyMateAI.Application.Features.Documents.Commands.UploadDocument
{
    public class UploadDocumentCommand : IRequest<DocumentResponseDto>
    {
        public int UserId { get; set; }
        public int SubjectId { get; set; }
        public Stream Content { get; set; } = Stream.Null;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public long Size { get; set; }
    }
}