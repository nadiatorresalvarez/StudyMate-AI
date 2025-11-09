using AutoMapper;
using StudyMateAI.Application.DTOs.Auth;
using StudyMateAI.Domain.Interfaces; // <-- Referencia a Domain
using System.Threading.Tasks;
using StudyMateAI.Application.DTOs;

namespace StudyMateAI.Application.UseCases.Auth
{
    public class GoogleAuthUseCase
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public GoogleAuthUseCase(IAuthService authService, IMapper mapper)
        {
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> ExecuteAsync(AuthRequestDto request)
        {
            var (user, jwtToken) = await _authService.AuthenticateWithGoogleAsync(request.GoogleIdToken);
            var userProfileDto = _mapper.Map<UserProfileDto>(user);
            return new AuthResponseDto(jwtToken, userProfileDto);
        }
    }
}