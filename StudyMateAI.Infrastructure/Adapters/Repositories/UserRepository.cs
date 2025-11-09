using Microsoft.EntityFrameworkCore;
using StudyMateAI.Domain.Entities;     // <-- Namespace Arreglado
using StudyMateAI.Domain.Interfaces;    // <-- Archivo que creaste en Paso 1
using StudyMateAI.Infrastructure.Data;
using System.Threading.Tasks;

namespace StudyMateAI.Infrastructure.Adapters.Repositories
{
    public class UserRepository : IUserRepository // <-- Ahora sí lo encuentra
    {
        private readonly dbContextStudyMateAI _context;

        public UserRepository(dbContextStudyMateAI context)
        {
            _context = context;
        }

        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public void Create(User user)
        {
            _context.Users.Add(user);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
        }
    }
}