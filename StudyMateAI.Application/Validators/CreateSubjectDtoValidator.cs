using FluentValidation;
using StudyMateAI.Application.DTOs.Subject;

namespace StudyMateAI.Application.Validators;

public class CreateSubjectDtoValidator : AbstractValidator<CreateSubjectDto>
{
    public CreateSubjectDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(3, 100);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.Icon)
            .MaximumLength(50);

        RuleFor(x => x.Color)
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .When(x => !string.IsNullOrWhiteSpace(x.Color));
    }
}