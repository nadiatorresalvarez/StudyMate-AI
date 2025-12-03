using StudyMateAI.Client.DTOs.Subject;

namespace StudyMateAI.Client.Services.Interfaces;

/// <summary>
/// Interfaz para servicios relacionados con materias
/// </summary>
public interface ISubjectService
{
    /// <summary>
    /// Obtiene todas las materias del usuario
    /// </summary>
    Task<List<SubjectResponseDto>> GetAll();

    /// <summary>
    /// Crea una nueva materia
    /// </summary>
    Task Create(CreateSubjectDto subject);

    /// <summary>
    /// Actualiza una materia existente
    /// </summary>
    Task Update(int id, UpdateSubjectDto subject);

    /// <summary>
    /// Elimina una materia
    /// </summary>
    Task Delete(int id);
}
