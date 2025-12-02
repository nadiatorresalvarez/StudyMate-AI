using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Application.Common.Exceptions;
using StudyMateAI.Application.DTOs.File;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Summaries.Queries;

public class GetResumenWordQuery : IRequest<FileDto>
{
    public int UserId { get; set; }
    public int ResumenId { get; set; }
}

internal class GetResumenWordQueryHandler : IRequestHandler<GetResumenWordQuery, FileDto>
{
    private readonly IRepository<Summary> _summaryRepository;
    private readonly IReportGenerator _reportGenerator;

    public GetResumenWordQueryHandler(
        IRepository<Summary> summaryRepository,
        IReportGenerator reportGenerator)
    {
        _summaryRepository = summaryRepository;
        _reportGenerator = reportGenerator;
    }

    public async Task<FileDto> Handle(GetResumenWordQuery request, CancellationToken cancellationToken)
    {
        // 1. OBTENER DATOS (con relaciones cargadas)
        var resumen = await _summaryRepository
            .Query()
            .Include(s => s.Document)
                .ThenInclude(d => d.Subject)
            .FirstOrDefaultAsync(s => s.Id == request.ResumenId, cancellationToken);

        if (resumen == null)
            throw new NotFoundException($"Resumen con ID {request.ResumenId} no encontrado.");

        // 2. VALIDAR PROPIEDAD DEL USUARIO
        if (resumen.Document?.Subject?.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("El resumen no pertenece al usuario.");
        }

        // 3. GENERAR CONTENIDO (Llama al Adaptador a través del Puerto)
        var fileContent = _reportGenerator.GenerateResumenWord(resumen);

        // 4. GENERAR NOMBRE DE ARCHIVO
        var documentName = resumen.Document?.OriginalFileName ?? "Documento";
        var documentNameWithoutExtension = Path.GetFileNameWithoutExtension(documentName);
        var summaryType = resumen.Type switch
        {
            "Brief" => "Resumen-Breve",
            "Detailed" => "Resumen-Detallado",
            "KeyConcepts" => "Conceptos-Clave",
            _ => "Resumen"
        };

        // 5. RETORNAR DTO
        return new FileDto
        {
            Content = fileContent,
            FileName = $"{summaryType}-{documentNameWithoutExtension}.docx",
            ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };
    }
}

