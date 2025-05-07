using MediatR;

public class RemoveTagFromTodoItemCommand : IRequest
{
    public int TodoItemId { get; set; }
    public string? TagName { get; set; }
}