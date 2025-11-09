using AutoMapper;
using StudyMateAI.Application.DTOs.Subject;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudyMateAI.Application.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public SubjectService(ISubjectRepository subjectRepository, IMapper mapper)
        {
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<SubjectResponseDto> CreateSubjectAsync(int userId, CreateSubjectDto createDto)
        {
            var subject = _mapper.Map<Subject>(createDto);
            subject.UserId = userId;
            subject.CreatedAt = DateTime.UtcNow;
            subject.IsArchived = false;

            var createdSubject = await _subjectRepository.AddAsync(subject);
            
            var response = _mapper.Map<SubjectResponseDto>(createdSubject);
            response.DocumentCount = 0;
            
            return response;
        }

        public async Task<SubjectResponseDto?> GetSubjectByIdAsync(int userId, int subjectId)
        {
            var subject = await _subjectRepository.GetByIdAndUserIdAsync(subjectId, userId);
            
            if (subject == null)
                return null;

            var response = _mapper.Map<SubjectResponseDto>(subject);
            response.DocumentCount = subject.Documents?.Count ?? 0;
            
            return response;
        }

        public async Task<IEnumerable<SubjectResponseDto>> GetAllSubjectsAsync(int userId)
        {
            var subjects = await _subjectRepository.GetByUserIdAsync(userId);
            
            return subjects.Select(s =>
            {
                var dto = _mapper.Map<SubjectResponseDto>(s);
                dto.DocumentCount = s.Documents?.Count ?? 0;
                return dto;
            });
        }

        public async Task<IEnumerable<SubjectResponseDto>> GetActiveSubjectsAsync(int userId)
        {
            var subjects = await _subjectRepository.GetActiveByUserIdAsync(userId);
            
            return subjects.Select(s =>
            {
                var dto = _mapper.Map<SubjectResponseDto>(s);
                dto.DocumentCount = s.Documents?.Count ?? 0;
                return dto;
            });
        }

        public async Task<SubjectResponseDto?> UpdateSubjectAsync(int userId, int subjectId, UpdateSubjectDto updateDto)
        {
            var subject = await _subjectRepository.GetByIdAndUserIdAsync(subjectId, userId);
            
            if (subject == null)
                return null;

            subject.Name = updateDto.Name;
            subject.Description = updateDto.Description;
            subject.Color = updateDto.Color;
            subject.Icon = updateDto.Icon;
            
            if (updateDto.OrderIndex.HasValue)
                subject.OrderIndex = updateDto.OrderIndex.Value;
            
            if (updateDto.IsArchived.HasValue)
                subject.IsArchived = updateDto.IsArchived.Value;

            var updatedSubject = await _subjectRepository.UpdateAsync(subject);
            
            var response = _mapper.Map<SubjectResponseDto>(updatedSubject);
            response.DocumentCount = updatedSubject.Documents?.Count ?? 0;
            
            return response;
        }

        public async Task<bool> DeleteSubjectAsync(int userId, int subjectId)
        {
            var subject = await _subjectRepository.GetByIdAndUserIdAsync(subjectId, userId);
            
            if (subject == null)
                return false;

            return await _subjectRepository.DeleteAsync(subjectId);
        }

        public async Task<bool> CanDeleteSubjectAsync(int subjectId)
        {
            return await _subjectRepository.HasDocumentsAsync(subjectId);
        }
    }
}
