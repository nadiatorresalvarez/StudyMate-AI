using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth;

namespace StudyMateAI.Infrastructure.Adapters.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<(User User, string JwtToken)> AuthenticateWithGoogleAsync(string idToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["GoogleAuth:ClientId"] }
            });

            var user = await _userRepository.GetByGoogleIdAsync(payload.Subject);

            if (user == null)
            {
                // ==========================================================
                // === ARREGLO PARA 'Name' cannot be null ===
                // ==========================================================
                // Si 'payload.Name' es nulo, usamos la parte del email antes del @
                string name = payload.Name;
                if (string.IsNullOrEmpty(name))
                {
                    name = payload.Email.Split('@')[0];
                }
                // ==========================================================

                user = new User(payload.Subject, payload.Email, name, payload.Picture);
                
                user = await _userRepository.CreateAsync(user);
            }
            else
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }

            var jwtToken = GenerateJwtToken(user);
            return (user, jwtToken);
        }

        public string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24), // Requisito de 24 horas
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}