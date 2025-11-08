using MediatR;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Users.Commands;

// =================================================================
    // 1. EL COMMAND (Público)
    // Representa la intención de realizar una acción de escritura.
    // =================================================================
    public class UpdateUserProfileCommand : IRequest<Unit>
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EducationLevel { get; set; } = string.Empty;
    }

    // =================================================================
    // 2. EL HANDLER (Interno)
    // Contiene la lógica para procesar el comando.
    // =================================================================
    internal class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            
            // TODO: Añadir manejo de excepción si el usuario no se encuentra.
            // if (user == null) throw new NotFoundException(...);

            // La capa de aplicación ORQUESTA, la capa de dominio EJECUTA la lógica de negocio.
            user.UpdateProfile(request.Name, request.EducationLevel);

            // El repositorio marca la entidad como modificada (si usas EF Core, el DbContext lo hace)
             _userRepository.Update(user); // Este método podría no ser async

            // El Unit of Work persiste todos los cambios en la transacción
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }