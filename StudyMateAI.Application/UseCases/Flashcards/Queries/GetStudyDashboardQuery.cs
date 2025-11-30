using MediatR;
using StudyMateAI.Application.DTOs.Flashcards;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Flashcards.Queries;

public record GetStudyDashboardQuery(int UserId) : IRequest<StudyDashboardDto>;

public class GetStudyDashboardQueryHandler : IRequestHandler<GetStudyDashboardQuery, StudyDashboardDto>
{
    private readonly IRepository<Flashcard> _flashcardRepository;

    public GetStudyDashboardQueryHandler(IRepository<Flashcard> flashcardRepository)
    {
        _flashcardRepository = flashcardRepository;
    }

    public async Task<StudyDashboardDto> Handle(GetStudyDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var all = await _flashcardRepository.FindAsync(f => f.Document.Subject.UserId == request.UserId);

        var overdue = all.Count(f => f.NextReviewDate.HasValue && f.NextReviewDate.Value.Date < today);
        var todayCount = all.Count(f => f.NextReviewDate.HasValue && f.NextReviewDate.Value.Date == today);
        var future = all.Count(f => f.NextReviewDate.HasValue && f.NextReviewDate.Value.Date > today);

        return new StudyDashboardDto
        {
            Total = all.Count(),
            Overdue = overdue,
            Today = todayCount,
            Future = future
        };
    }
}
