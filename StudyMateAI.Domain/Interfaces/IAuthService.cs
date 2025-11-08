// 1. Namespace
namespace StudyMateAI.Domain.Interfaces
{
    // 2. Importa la entidad User (después de que arregles su namespace)
    using StudyMateAI.Domain.Entities;
    using System.Threading.Tasks;

    public interface IAuthService
    {
        // Modificado para devolver el User y el Token
        Task<(User User, string JwtToken)> AuthenticateWithGoogleAsync(string idToken);
        string GenerateJwtToken(User user);
    }
}