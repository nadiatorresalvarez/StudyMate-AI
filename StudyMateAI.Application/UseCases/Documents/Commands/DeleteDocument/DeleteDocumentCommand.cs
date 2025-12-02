using MediatR;

namespace StudyMateAI.Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public int DocumentId { get; set; }

        public DeleteDocumentCommand(int userId, int documentId)
        {
            UserId = userId;
            DocumentId = documentId;
        }
    }
}

