using FluentValidation;
using StudyMateAI.DTOs.Request;

namespace StudyMateAI.Validators;

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(3, 50);

        RuleFor(x => x.EducationLevel)
            .NotEmpty();
    }
}
