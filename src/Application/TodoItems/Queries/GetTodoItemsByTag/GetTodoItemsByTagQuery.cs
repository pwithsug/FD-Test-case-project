using MediatR;

public class GetTodoItemsByTagQuery : IRequest<List<TodoItemWithTagsDto>>
{
    public string? TagName { get; set;}
}