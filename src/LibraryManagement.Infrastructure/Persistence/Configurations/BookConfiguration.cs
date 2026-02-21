using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.ISBN)
            .HasMaxLength(20);

        builder.Property(b => b.Description)
            .HasMaxLength(5000);

        builder.Property(b => b.CoverImageUrl)
            .HasMaxLength(2000);

        builder.Property(b => b.Publisher)
            .HasMaxLength(200);

        builder.Property(b => b.Language)
            .HasMaxLength(50);

        builder.HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(b => !b.IsDeleted);

        builder.HasIndex(b => b.ISBN).IsUnique().HasFilter("\"ISBN\" IS NOT NULL");
        builder.HasIndex(b => b.Title);
        builder.HasIndex(b => b.AuthorId);

        builder.Ignore(b => b.DomainEvents);
    }
}
