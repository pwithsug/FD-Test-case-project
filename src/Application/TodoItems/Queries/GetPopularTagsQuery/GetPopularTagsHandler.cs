using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Interfaces;

public class GetPopularTagsQueryHandler : IRequestHandler<GetPopularTagsQuery, List<TagDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPopularTagsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<TagDto>> Handle(GetPopularTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _context.Tags
            .Include(t => t.TodoItems)
            .OrderByDescending(t => t.TodoItems.Count)
            .Take(request.Count)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<TagDto>>(tags);
    }
}