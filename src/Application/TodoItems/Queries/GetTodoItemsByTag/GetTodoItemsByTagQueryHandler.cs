using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Interfaces;

public class GetTodoItemsByTagQueryHandler : IRequestHandler<GetTodoItemsByTagQuery, List<TodoItemWithTagsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTodoItemsByTagQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<TodoItemWithTagsDto>> Handle(GetTodoItemsByTagQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TodoItems
            .Include(t => t.Tags)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.TagName))
        {
            query = query.Where(item => item.Tags.Any(t => t.Name == request.TagName));
        }

        var items = await query.ToListAsync(cancellationToken);
        return _mapper.Map<List<TodoItemWithTagsDto>>(items);
    }
}