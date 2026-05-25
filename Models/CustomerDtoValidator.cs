using FluentValidation;

namespace New_folder.Models;

public class CustomerDtoValidator : AbstractValidator<CustomerDto>
{
    public CustomerDtoValidator()
    {
        RuleFor(x => x.Gender).NotNull().NotEmpty();
        RuleFor(x => x.Age).NotNull();
        RuleFor(x => x.AnnualIncome).NotNull();
        RuleFor(x => x.SpendingScore).NotNull();
        RuleFor(x => x.Profession).NotNull().NotEmpty();
        RuleFor(x => x.WorkExperience).NotNull();
        RuleFor(x => x.FamilySize).NotNull();
    }
}
