using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Application.TodoLists.Commands.CreateTodoList;
using Todo_App.Application.TodoLists.Commands.DeleteTodoList;
using Todo_App.Application.TodoLists.Commands.UpdateTodoList;
using Todo_App.Application.TodoLists.Queries.ExportTodos;
using Todo_App.Application.TodoLists.Queries.GetTodos;


namespace Todo_App.WebUI.Controllers;

public class TodoListsController : ApiControllerBase
{

    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public TodoListsController(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<TodosVm>> Get()
    {
        return await Mediator.Send(new GetTodosQuery());
    }

    [HttpGet("{id}")]
    public async Task<FileResult> Get(int id)
    {
        var vm = await Mediator.Send(new ExportTodosQuery { ListId = id });

        return File(vm.Content, vm.ContentType, vm.FileName);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateTodoListCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateTodoListCommand command)
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
        await Mediator.Send(new DeleteTodoListCommand(id));

        return NoContent();
    }

    [HttpGet("deleted")]
    public async Task<ActionResult<List<TodoListDto>>> GetDeletedLists()
    {
        return await _context.TodoLists
            .IgnoreQueryFilters()
            .Where(l => l.IsDeleted)
            .AsNoTracking()
            .ProjectTo<TodoListDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    [HttpPut("{id}/restore")]
    public async Task<ActionResult> RestoreList(int id)
    {
        var list = await _context.TodoLists
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted);

        if (list == null) return NotFound();

        list.IsDeleted = false;
        list.DeletedAt = null;

        await _context.SaveChangesAsync(CancellationToken.None);
        return NoContent();
    }
}
