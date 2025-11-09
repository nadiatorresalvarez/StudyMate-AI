using AutoMapper;
using StudyMateAI.Application.DTOs;
using StudyMateAI.Domain.Entities; // <-- Referencia a Domain

namespace StudyMateAI.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Mapeo de Entidad User -> UserProfileDto
            CreateMap<User, UserProfileDto>();
        }
    }
}