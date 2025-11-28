using AutoMapper;
using StudyMateAI.Application.DTOs;
using StudyMateAI.Application.DTOs.Auth;
using StudyMateAI.Application.DTOs.Subject;
using StudyMateAI.Application.DTOs.Document;
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

            // Mapeos de Document
            CreateMap<CreateDocumentDto, Document>();
            CreateMap<Document, DocumentResponseDto>()
                .ForMember(dest => dest.SubjectName, opt => opt.Ignore())
                .ForMember(dest => dest.FlashcardCount, opt => opt.Ignore())
                .ForMember(dest => dest.QuizCount, opt => opt.Ignore())
                .ForMember(dest => dest.SummaryCount, opt => opt.Ignore())
                .ForMember(dest => dest.MindmapCount, opt => opt.Ignore())
                .ForMember(dest => dest.ConceptmapCount, opt => opt.Ignore());
        }
    }
}