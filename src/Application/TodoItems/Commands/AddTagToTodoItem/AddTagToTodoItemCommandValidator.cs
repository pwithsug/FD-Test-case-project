using System.Data;
using FluentValidation;

namespace Todo_App.Application.TodoItems.Commands.AddTagToTodoItem;

public class AddTagToTodoItemCommandValidator : AbstractValidator<AddTagToTodoItemCommand>
{
    public AddTagToTodoItemCommandValidator(){
        RuleFor(x => x.TodoItemId)
            .NotEmpty()
            .WithMessage("TodoItemId is required.");
        RuleFor(x => x.TagName)
        .NotEmpty()
        .MaximumLength(50)
        .WithMessage("TagName must not exceed 50 characters.");
        RuleFor(x => x.TagColor)
        .Matches("^#(?:[0-9a-fA-F]{3}){1,2}$") // Hex color code validation
        .When(v => !string.IsNullOrEmpty(v.TagColor));
    }
}