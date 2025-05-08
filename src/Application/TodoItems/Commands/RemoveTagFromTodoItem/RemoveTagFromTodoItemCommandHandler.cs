using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Entities;

public class RemoveTagFromTodoItemCommandHandler : IRequestHandler<RemoveTagFromTodoItemCommand>
{
    private readonly IApplicationDbContext _context;

    public RemoveTagFromTodoItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RemoveTagFromTodoItemCommand request, CancellationToken cancellationToken)
    {
        var todoItem = await _context.TodoItems
            .Include(i => i.Tags)
            .FirstOrDefaultAsync(i => i.Id == request.TodoItemId, cancellationToken);

        if (todoItem == null)
        {
            throw new NotFoundException(nameof(TodoItem), request.TodoItemId);
        }

        var tag = todoItem.Tags.FirstOrDefault(t => t.Name == request.TagName);
        if (tag != null)
        {
            todoItem.Tags.Remove(tag);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}