using System.Reflection;
using AutoMapper;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapeo básico para Tags
            CreateMap<Tag, string>().ConvertUsing(t => t.Name ?? string.Empty);
            
            // Mapeo para TodoItemWithTagsDto
            CreateMap<TodoItem, TodoItemWithTagsDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));
            
            // Añade esto para el conteo en TagDto
            CreateMap<Tag, TagDto>()
                .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.TodoItems.Count));
                
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
