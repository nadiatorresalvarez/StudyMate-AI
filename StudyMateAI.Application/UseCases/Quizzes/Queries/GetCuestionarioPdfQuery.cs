using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Application.Common.Exceptions;
using StudyMateAI.Application.DTOs.File;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Quizzes.Queries;

public class GetCuestionarioPdfQuery : IRequest<FileDto>
{
    public int UserId { get; set; }
    public int CuestionarioId { get; set; }
}

internal class GetCuestionarioPdfQueryHandler : IRequestHandler<GetCuestionarioPdfQuery, FileDto>
{
    private readonly IRepository<Quiz> _quizRepository;
    private readonly IReportGenerator _reportGenerator;

    public GetCuestionarioPdfQueryHandler(
        IRepository<Quiz> quizRepository,
        IReportGenerator reportGenerator)
    {
        _quizRepository = quizRepository;
        _reportGenerator = reportGenerator;
    }

    public async Task<FileDto> Handle(GetCuestionarioPdfQuery request, CancellationToken cancellationToken)
    {
        // 1. OBTENER DATOS (con relaciones cargadas)
        var cuestionario = await _quizRepository
            .Query()
            .Include(q => q.Document)
                .ThenInclude(d => d.Subject)
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == request.CuestionarioId, cancellationToken);

        if (cuestionario == null)
            throw new NotFoundException($"Cuestionario con ID {request.CuestionarioId} no encontrado.");

        // 2. VALIDAR PROPIEDAD DEL USUARIO
        if (cuestionario.Document?.Subject?.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("El cuestionario no pertenece al usuario.");
        }

        // 3. GENERAR CONTENIDO (Llama al Adaptador a través del Puerto)
        var fileContent = _reportGenerator.GenerateCuestionarioPdf(cuestionario);

        // 4. GENERAR NOMBRE DE ARCHIVO
        var documentName = cuestionario.Document?.OriginalFileName ?? "Documento";
        var documentNameWithoutExtension = Path.GetFileNameWithoutExtension(documentName);
        var safeTitle = cuestionario.Title.Replace(" ", "-").Replace("/", "-").Replace("\\", "-");

        // 5. RETORNAR DTO
        return new FileDto
        {
            Content = fileContent,
            FileName = $"Cuestionario-{safeTitle}-{documentNameWithoutExtension}.pdf",
            ContentType = "application/pdf"
        };
    }
}

