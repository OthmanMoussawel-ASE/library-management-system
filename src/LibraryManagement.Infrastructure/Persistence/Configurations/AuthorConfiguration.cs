using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Infrastructure.Persistence.Configurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FirstName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.LastName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Biography)
            .HasMaxLength(5000);

        builder.HasQueryFilter(a => !a.IsDeleted);
        builder.Ignore(a => a.DomainEvents);
    }
}
