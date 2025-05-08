using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Entities;
using Todo_App.Domain.Enums;

namespace Todo_App.Application.TodoItems.Commands.UpdateTodoItemDetail;

public record UpdateTodoItemDetailCommand : IRequest
{
    public int Id { get; init; }

    public int ListId { get; init; }

    public PriorityLevel Priority { get; init; }

    public string? Note { get; init; }

    public string? BackgroundColor { get; set; } = "#ffffff";
    public List<string> Tags { get; set; } = new List<string>();
}

public class UpdateTodoItemDetailCommandHandler : IRequestHandler<UpdateTodoItemDetailCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateTodoItemDetailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateTodoItemDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TodoItems
            .Include(item => item.Tags) 
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(TodoItem), request.Id);
        }

        entity.ListId = request.ListId;
        entity.Priority = request.Priority;
        entity.Note = request.Note;
        entity.BackgroundColor = request.BackgroundColor;

        await UpdateTags(entity, request.Tags ?? new List<string>(), cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async Task UpdateTags(TodoItem todoItem, List<string> newTagNames, CancellationToken cancellationToken)
    {
        todoItem.Tags.Clear();

        var existingTags = await _context.Tags
            .Where(t => newTagNames.Contains(t.Name))
            .ToListAsync(cancellationToken);

        foreach (var tagName in newTagNames)
        {
            var existingTag = existingTags.FirstOrDefault(t => t.Name == tagName);

            if (existingTag != null)
            {
                todoItem.Tags.Add(existingTag);
            }
            else
            {
                var newTag = new Tag
                {
                    Name = tagName,
                    Color = GetDefaultTagColor()
                };
                todoItem.Tags.Add(newTag);
            }
        }
    }

    private string GetDefaultTagColor()
    {
        return "#6b7280";
    }
}