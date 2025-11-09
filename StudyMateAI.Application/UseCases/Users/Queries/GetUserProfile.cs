using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Users.Queries;

public class GetUserProfileQuery : IRequest<UserProfileDto>
{
    public int UserId { get; set; }
}

// =================================================================
// 2. EL HANDLER (Interno)
// Contiene la lógica. Al ser 'internal', solo es visible dentro
// de este proyecto (Application) y MediatR lo encontrará
// automáticamente por reflexión.
// =================================================================
internal class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUserProfileQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);

        // TODO: Añadir manejo de excepción si el usuario no se encuentra.
        // if (user == null) throw new NotFoundException(...);

        return _mapper.Map<UserProfileDto>(user);
    }
}