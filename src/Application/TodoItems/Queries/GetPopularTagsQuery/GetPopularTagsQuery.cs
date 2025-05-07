using MediatR;

public class GetPopularTagsQuery : IRequest<List<TagDto>>
{
    public int Count { get; set; } = 5;
}