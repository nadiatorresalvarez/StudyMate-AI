using AutoMapper;
using StudyMateAI.Application.DTOs;
using StudyMateAI.Application.DTOs.Auth;
using StudyMateAI.Application.DTOs.Subject;
using StudyMateAI.Domain.Entities; // <-- Referencia a Domain

namespace StudyMateAI.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Mapeo de Entidad User -> UserProfileDto
            CreateMap<User, UserProfileDto>();

            // Mapeos de Subject
            CreateMap<CreateSubjectDto, Subject>();
            CreateMap<Subject, SubjectResponseDto>()
                .ForMember(dest => dest.DocumentCount, opt => opt.Ignore());
        }
    }
}