using Ecommerce.Kernel.Infrastructure.Messaging.Inbox;
using Ecommerce.Kernel.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Kernel.Infrastructure.Messaging;

public static class MessagingModelBuilderExtensions
{
    private const int NameMaxLength = 200;

    public static ModelBuilder ApplyMessagingTables(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(outbox =>
        {
            outbox.ToTable("OutboxMessages");
            outbox.HasKey(message => message.Id);
            outbox.Property(message => message.Id).ValueGeneratedNever();
            outbox.Property(message => message.EventName).HasMaxLength(NameMaxLength).IsRequired();
            outbox.Property(message => message.Payload).HasColumnType("jsonb").IsRequired();
            outbox.Property(message => message.OccurredOnUtc).IsRequired();
            outbox.HasIndex(message => new { message.OccurredOnUtc, message.Id });
        });

        modelBuilder.Entity<InboxMessage>(inbox =>
        {
            inbox.ToTable("InboxMessages");
            inbox.HasKey(message => new { message.MessageId, message.Consumer });
            inbox.Property(message => message.Consumer).HasMaxLength(NameMaxLength);
            inbox.Property(message => message.ProcessedOnUtc).IsRequired();
        });

        return modelBuilder;
    }
}
