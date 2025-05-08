using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Models;
using Todo_App.Application.TodoItems.Commands.CreateTodoItem;
using Todo_App.Application.TodoItems.Commands.DeleteTodoItem;
using Todo_App.Application.TodoItems.Commands.UpdateTodoItem;
using Todo_App.Application.TodoItems.Commands.UpdateTodoItemDetail;
using Todo_App.Application.TodoItems.Queries.GetTodoItemsWithPagination;
using Todo_App.Application.TodoLists.Queries.GetTodos;
using Todo_App.Infrastructure.Persistence;
using AutoMapper.QueryableExtensions;
using AutoMapper;

namespace Todo_App.WebUI.Controllers;

public class TodoItemsController : ApiControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public TodoItemsController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<TodoItemBriefDto>>> GetTodoItemsWithPagination([FromQuery] GetTodoItemsWithPaginationQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateTodoItemCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateTodoItemCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpPut("[action]")]
    public async Task<ActionResult> UpdateItemDetails(int id, UpdateTodoItemDetailCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteTodoItemCommand(id));

        return NoContent();
    }

    [HttpPost("{id}/tags")]
    public async Task<ActionResult> AddTag(int id, AddTagToTodoItemCommand command)
    {
        if (id != command.TodoItemId)
        {
            return BadRequest();
        }

        await Mediator.Send(command);
        return NoContent();
    }

    [HttpGet("by-tag")]
    public async Task<ActionResult<List<TodoItemWithTagsDto>>> GetByTag([FromQuery] GetTodoItemsByTagQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpDelete("{id}/tags/{tagName}")]
    public async Task<ActionResult> RemoveTag(int id, string tagName)
    {
        await Mediator.Send(new RemoveTagFromTodoItemCommand
        {
            TodoItemId = id,
            TagName = tagName
        });
        return NoContent();
    }

    [HttpGet("popular-tags")]
    public async Task<ActionResult<List<TagDto>>> GetPopularTags([FromQuery] int count = 5)
    {
        return await Mediator.Send(new GetPopularTagsQuery { Count = count });
    }

    [HttpGet("available-tags")]
    public async Task<ActionResult<List<string>>> GetAvailableTags()
    {
        var tags = await _context.Tags
            .Select(t => t.Name)
            .Distinct()
            .ToListAsync();
        return Ok(tags);
    }

    [HttpPut("[action]")]
    public async Task<ActionResult> UpdateItemDetailsWithTags(int id, UpdateTodoItemDetailCommand command)
    {
        if (id != command.Id) return BadRequest();

        var item = await Mediator.Send(command);

        return Ok(item);
    }

    [HttpGet("deleted")]
    public async Task<ActionResult<List<TodoItemDto>>> GetDeletedItems()
    {
        return await _context.TodoItems
            .IgnoreQueryFilters()
            .Where(i => i.IsDeleted)
            .ProjectTo<TodoItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    [HttpPut("{id}/restore")]
    public async Task<ActionResult> RestoreItem(int id)
    {
        var item = await _context.TodoItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Id == id && i.IsDeleted);

        if (item == null) return NotFound();

        item.IsDeleted = false;
        item.DeletedAt = null;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
