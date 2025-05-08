namespace Todo_App.Domain.Entities;

public class TodoItem : BaseAuditableEntity
{
    public int ListId { get; set; }

    public string? Title { get; set; }

    public string? Note { get; set; }

    public PriorityLevel Priority { get; set; }

    public DateTime? Reminder { get; set; }

    private bool _done;
    public bool Done
    {
        get => _done;
        set
        {
            if (value == true && _done == false)
            {
                AddDomainEvent(new TodoItemCompletedEvent(this));
            }

            _done = value;
        }
    }
    public string? BackgroundColor { get; set; } = "#ffffff";
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();


    public TodoList List { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
