
using MediatR;

public class AddTagToTodoItemCommand : IRequest
{
    public int TodoItemId { get; set; }
    public string? TagName { get; set; }
    public string? TagColor { get; set; }
}

public class AddTagToTodoItemCommandHandler : IRequestHandler<AddTagToTodoItemCommand>
{
    public Task Handle(AddTagToTodoItemCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    Task<Unit> IRequestHandler<AddTagToTodoItemCommand, Unit>.Handle(AddTagToTodoItemCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}