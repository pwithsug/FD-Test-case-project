using AutoMapper;
using Todo_App.Application.Common.Mappings;
using Todo_App.Domain.Entities;

public class TodoItemWithTagsDto : IMapFrom<TodoItem>
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public List<string> Tags { get; set; } = new List<string>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<TodoItem, TodoItemWithTagsDto>()
            .ForMember(d => d.Tags, 
                opt => opt.MapFrom(s => s.Tags.Select(t => t.Name).ToList()));
    }
}

public class TagDto : IMapFrom<Tag>
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Color { get; set; }
    public int Count { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Tag, TagDto>()
            .ForMember(d => d.Count, opt => opt.MapFrom(s => s.TodoItems.Count));
    }
}