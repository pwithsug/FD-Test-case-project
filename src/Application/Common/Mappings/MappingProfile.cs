using System.Reflection;
using AutoMapper;
using Todo_App.Application.TodoLists.Queries.GetTodos;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
            CreateMap<Tag, string>().ConvertUsing(t => t.Name ?? string.Empty);
            
            CreateMap<TodoItem, TodoItemWithTagsDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));
            
            CreateMap<Tag, TagDto>()
                .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.TodoItems.Count));
            
            CreateMap<TodoItem, TodoItemDto>()
            .ForMember(d => d.IsDeleted, opt => opt.MapFrom(s => s.IsDeleted))
            .ForMember(d => d.DeletedAt, opt => opt.MapFrom(s => s.DeletedAt));

            CreateMap<TodoList, TodoListDto>()
            .ForMember(d => d.IsDeleted, opt => opt.MapFrom(s => s.IsDeleted))
            .ForMember(d => d.DeletedAt, opt => opt.MapFrom(s => s.DeletedAt));
                
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);

        var mappingMethodName = nameof(IMapFrom<object>.Mapping);

        bool HasInterface(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == mapFromType;

        var types = assembly.GetExportedTypes().Where(t => t.GetInterfaces().Any(HasInterface)).ToList();

        var argumentTypes = new Type[] { typeof(Profile) };

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);

            var methodInfo = type.GetMethod(mappingMethodName);

            if (methodInfo != null)
            {
                methodInfo.Invoke(instance, new object[] { this });
            }
            else
            {
                var interfaces = type.GetInterfaces().Where(HasInterface).ToList();

                if (interfaces.Count > 0)
                {
                    foreach (var @interface in interfaces)
                    {
                        var interfaceMethodInfo = @interface.GetMethod(mappingMethodName, argumentTypes);

                        interfaceMethodInfo?.Invoke(instance, new object[] { this });
                    }
                }
            }
        }
    }
}
