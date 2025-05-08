using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Todo_App.Domain.Entities;

namespace Todo_App.Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags"); 
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(t => t.Color)
            .HasMaxLength(20)
            .HasDefaultValue("#6b7280");
            
        builder.HasMany(t => t.TodoItems)
            .WithMany(t => t.Tags)
            .UsingEntity<Dictionary<string, object>>(
                "TagTodoItem",
                j => j.HasOne<TodoItem>().WithMany().HasForeignKey("TodoItemsId"),
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagsId"),
                j => j.ToTable("TagTodoItem"));
    }
}