using MediatR;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, bool>
    {
        private readonly IDocumentRepository _documentRepository;

        public DeleteDocumentCommandHandler(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el documento pertenece al usuario
            var userOwnsDocument = await _documentRepository.UserOwnsDocumentAsync(request.DocumentId, request.UserId);
            if (!userOwnsDocument)
                return false;

            var document = await _documentRepository.GetByIdAsync(request.DocumentId);
            if (document == null)
                return false;

            await _documentRepository.DeleteAsync(document.Id);
            return true;
        }
    }
}
