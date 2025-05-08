using NUnit.Framework;
using Moq;
using Todo_App.Application.TodoItems.Commands.UpdateTodoItemDetail;
using Todo_App.Domain.Entities;
using Todo_App.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Todo_App.Domain.Enums;

namespace Todo_App.Application.UnitTests.TodoItems.Commands;

[TestFixture]
public class UpdateTodoItemDetailCommandTests
{
    private Mock<IApplicationDbContext>? _mockContext;
    private UpdateTodoItemDetailCommandHandler? _handler;
    private List<TodoItem>? _todoItems;

    [SetUp]
    public void Setup()
    {
        _todoItems = new List<TodoItem>
        {
            new TodoItem
            {
                Id = 1,
                BackgroundColor = "#ffffff",
                ListId = 1,
                Priority = PriorityLevel.None,
                Note = "Original note"
            }
        };

        var mockSet = new Mock<DbSet<TodoItem>>();

        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((object[] ids, CancellationToken token) =>
                  _todoItems.FirstOrDefault(x => x.Id == (int)ids[0]));

        _mockContext = new Mock<IApplicationDbContext>();
        _mockContext.Setup(c => c.TodoItems).Returns(mockSet.Object);

        _handler = new UpdateTodoItemDetailCommandHandler(_mockContext.Object);
    }

    [Test]
    public async Task Handle_Should_Update_BackgroundColor()
    {
        // Arrange
        var command = new UpdateTodoItemDetailCommand
        {
            Id = 1,
            BackgroundColor = "#00ff00",
            ListId = 1,
            Priority = PriorityLevel.High,
            Note = "Updated note"
        };

        // Act
        if (_handler != null)
        {
            await _handler.Handle(command, CancellationToken.None);
        }

        // Assert
        var updatedItem = _todoItems?.First();
        Assert.Multiple(() =>
        {
            Assert.That(updatedItem?.BackgroundColor, Is.EqualTo("#00ff00")); 
            Assert.That(updatedItem?.Priority, Is.EqualTo(PriorityLevel.High));
            Assert.That(updatedItem?.Note, Is.EqualTo("Updated note")); 
        });
    }
}