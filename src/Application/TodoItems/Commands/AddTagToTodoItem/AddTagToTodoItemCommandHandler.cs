using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.TodoItems.Commands.AddTagToTodoItem;

public class AddTagToTodoItemCommandHandler : IRequestHandler<AddTagToTodoItemCommand>
{
    private readonly IApplicationDbContext _context;

    public AddTagToTodoItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(AddTagToTodoItemCommand request, CancellationToken cancellationToken)
    {
        var todoItem = await _context.TodoItems
            .Include(i => i.Tags)
            .FirstOrDefaultAsync(i => i.Id == request.TodoItemId, cancellationToken);

        if (todoItem == null)
        {
            throw new NotFoundException(nameof(TodoItem), request.TodoItemId);
        }

        
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == request.TagName.ToLower(), cancellationToken);

        if (tag == null)
        {
            tag = new Tag
            {
                Name = request.TagName,
                Color = request.TagColor ?? "#6b7280"
            };
            _context.Tags.Add(tag);
        }

        if (!todoItem.Tags.Any(t => t.Id == tag.Id))
        {
            todoItem.Tags.Add(tag);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}