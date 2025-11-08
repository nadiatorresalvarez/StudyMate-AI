// 1. Asegúrate que el namespace sea el de tu proyecto/carpeta
namespace StudyMateAI.Domain.Interfaces
{
    // 2. Importa la entidad User (después de que arregles su namespace)
    using StudyMateAI.Domain.Entities;
    using System.Threading.Tasks;

    public interface IUserRepository
    {
        Task<User?> GetByGoogleIdAsync(string googleId);
        Task<User?> GetByIdAsync(int id);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
    }
}