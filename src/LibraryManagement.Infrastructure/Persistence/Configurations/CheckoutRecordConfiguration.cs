using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Infrastructure.Persistence.Configurations;

public class CheckoutRecordConfiguration : IEntityTypeConfiguration<CheckoutRecord>
{
    public void Configure(EntityTypeBuilder<CheckoutRecord> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasOne(c => c.Book)
            .WithMany(b => b.CheckoutRecords)
            .HasForeignKey(c => c.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Patron)
            .WithMany(p => p.CheckoutRecords)
            .HasForeignKey(c => c.PatronId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.BookId, c.PatronId, c.Status });
        builder.HasIndex(c => c.DueDate);
        builder.Ignore(c => c.DomainEvents);
    }
}
