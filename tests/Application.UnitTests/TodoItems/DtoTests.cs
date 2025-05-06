using NUnit.Framework;
using AutoMapper;
using Todo_App.Application.TodoLists.Queries.GetTodos;
using Todo_App.Domain.Entities;
using Todo_App.Application.Common.Mappings;

namespace Todo_App.Application.UnitTests.TodoItems.Dtos;

[TestFixture]
public class TodoItemDtoTests
{
    private IMapper? _mapper;

    [SetUp]
    public void Setup()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.AddProfile<MappingProfile>());
        _mapper = configuration.CreateMapper();
    }

    [Test]
    public void Should_Map_BackgroundColor_Correctly()
    {
        // Arrange
        var entity = new TodoItem { BackgroundColor = "#ff0000" };

        // Act
        var dto = _mapper?.Map<TodoItemDto>(entity);

        // Assert
        Assert.That(dto?.BackgroundColor, Is.EqualTo("#ff0000"));
    }
}