using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Infrastructure.Persistence.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder");
        const int maxRetries = 5;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                await context.Database.MigrateAsync();

                await SeedRolesAsync(roleManager);
                await SeedAdminUserAsync(userManager, context);
                await SeedLibrarianUserAsync(userManager, context);
                await SeedCategoriesAsync(context);
                await SeedAuthorsAndBooksAsync(context);

                logger.LogInformation("Database seeded successfully");
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                logger.LogWarning(ex, "Seed attempt {Attempt}/{Max} failed. Retrying in {Delay}s...", attempt, maxRetries, delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Librarian", "Patron"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        const string adminEmail = "admin@library.com";
        if (await userManager.FindByEmailAsync(adminEmail) is not null) return;

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(admin, "Admin@123!");
        await userManager.AddToRoleAsync(admin, "Admin");

        var patron = new Patron
        {
            UserId = admin.Id,
            FullName = "Admin User",
            Email = adminEmail,
            MembershipNumber = "LIB-ADMIN-000001"
        };
        context.Patrons.Add(patron);
        await context.SaveChangesAsync();
    }

    private static async Task SeedLibrarianUserAsync(UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        const string librarianEmail = "librarian@library.com";
        if (await userManager.FindByEmailAsync(librarianEmail) is not null) return;

        var librarian = new ApplicationUser
        {
            UserName = librarianEmail,
            Email = librarianEmail,
            FirstName = "Library",
            LastName = "Staff",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(librarian, "Librarian@123!");
        await userManager.AddToRoleAsync(librarian, "Librarian");

        var patron = new Patron
        {
            UserId = librarian.Id,
            FullName = "Library Staff",
            Email = librarianEmail,
            MembershipNumber = "LIB-STAFF-000001"
        };
        context.Patrons.Add(patron);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var categories = new List<Category>
        {
            new() { Name = "Fiction", Description = "Literary fiction and novels" },
            new() { Name = "Non-Fiction", Description = "Factual and informative books" },
            new() { Name = "Science Fiction", Description = "Speculative fiction involving science and technology" },
            new() { Name = "Fantasy", Description = "Imaginative fiction with magical elements" },
            new() { Name = "Mystery", Description = "Crime and detective fiction" },
            new() { Name = "Biography", Description = "Life stories of notable people" },
            new() { Name = "History", Description = "Historical accounts and analysis" },
            new() { Name = "Technology", Description = "Books about computing, engineering, and tech" },
            new() { Name = "Self-Help", Description = "Personal development and growth" },
            new() { Name = "Science", Description = "Scientific discoveries and knowledge" }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAuthorsAndBooksAsync(AppDbContext context)
    {
        if (await context.Books.AnyAsync()) return;

        var authorsExist = await context.Authors.AnyAsync();

        var categories = await context.Categories.ToListAsync();

        Author orwell, tolkien, asimov, christie, hawking;

        if (!authorsExist)
        {
            orwell = new Author { FirstName = "George", LastName = "Orwell", Biography = "English novelist, essayist, and critic." };
            tolkien = new Author { FirstName = "J.R.R.", LastName = "Tolkien", Biography = "English writer and philologist, best known for The Hobbit and The Lord of the Rings." };
            asimov = new Author { FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry known for science fiction works." };
            christie = new Author { FirstName = "Agatha", LastName = "Christie", Biography = "English writer known for detective novels." };
            hawking = new Author { FirstName = "Stephen", LastName = "Hawking", Biography = "English theoretical physicist and cosmologist." };

            context.Authors.AddRange(orwell, tolkien, asimov, christie, hawking);
            await context.SaveChangesAsync();
        }
        else
        {
            var authors = await context.Authors.ToListAsync();
            orwell = authors.First(a => a.LastName == "Orwell");
            tolkien = authors.First(a => a.LastName == "Tolkien");
            asimov = authors.First(a => a.LastName == "Asimov");
            christie = authors.First(a => a.LastName == "Christie");
            hawking = authors.First(a => a.LastName == "Hawking");
        }

        var fiction = categories.First(c => c.Name == "Fiction");
        var sciFi = categories.First(c => c.Name == "Science Fiction");
        var fantasy = categories.First(c => c.Name == "Fantasy");
        var mystery = categories.First(c => c.Name == "Mystery");
        var science = categories.First(c => c.Name == "Science");

        var books = new List<Book>
        {
            new()
            {
                Title = "1984", ISBN = "978-0451524935", AuthorId = orwell.Id,
                Description = "A dystopian novel set in a totalitarian society ruled by Big Brother.",
                TotalCopies = 5, AvailableCopies = 5, PublishedDate = new DateTime(1949, 6, 8, 0, 0, 0, DateTimeKind.Utc), Language = "English",
                BookCategories = [new() { CategoryId = fiction.Id }, new() { CategoryId = sciFi.Id }]
            },
            new()
            {
                Title = "Animal Farm", ISBN = "978-0451526342", AuthorId = orwell.Id,
                Description = "A satirical allegorical novella reflecting events leading to the Russian Revolution.",
                TotalCopies = 3, AvailableCopies = 3, PublishedDate = new DateTime(1945, 8, 17, 0, 0, 0, DateTimeKind.Utc), Language = "English",
                BookCategories = [new() { CategoryId = fiction.Id }]
            },
            new()
            {
                Title = "The Hobbit", ISBN = "978-0547928227", AuthorId = tolkien.Id,
                Description = "A fantasy novel about the adventures of Bilbo Baggins.",
                TotalCopies = 4, AvailableCopies = 4, PublishedDate = new DateTime(1937, 9, 21, 0, 0, 0, DateTimeKind.Utc), Language = "English",
                BookCategories = [new() { CategoryId = fantasy.Id }, new() { CategoryId = fiction.Id }]
            },
            new()
            {
                Title = "The Lord of the Rings", ISBN = "978-0618640157", AuthorId = tolkien.Id,
                Description = "An epic high-fantasy novel that follows hobbits on their quest to destroy the One Ring.",
                TotalCopies = 3, AvailableCopies = 3, PublishedDate = new DateTime(1954, 7, 29, 0, 0, 0, DateTimeKind.Utc), Language = "English",
                BookCategories = [new() { CategoryId = fantasy.Id }, new() { CategoryId = fiction.Id }]
            },
            new()
            {
                Title = "Foundation", ISBN = "978-0553293357", AuthorId = asimov.Id,
                Description = "A science fiction novel about the fall of a galactic empire and a plan to shorten the dark age.",
                TotalCopies = 3, AvailableCopies = 3, PublishedDate = new DateTime(1951, 5, 1, 0, 0, 0, DateTimeKind.Utc), Language = "English",
                BookCategories = [new() { CategoryId = sciFi.Id }, new() { CategoryId = fiction.Id }]
            },
            new()
            {
                Title = "I, Robot", ISBN = "978-0553382563", AuthorId = asimov.Id,
                Description = "A collection of science fiction short stories about robots and their interactions with humans.",
                TotalCopies = 2, AvailableCopies = 2, PublishedDate = new DateTime(1950, 12, 2, 0, 0, 0, DateTimeKind.Utc), Language = "English",
                BookCategories = [new() { CategoryId = sciFi.Id }]
            },
            new()
            {
                Title = "Murder on the Orient Express", ISBN = "978-0062693662", AuthorId = christie.Id,
                Description = "A detective novel featuring Hercule Poirot investigating a murder on a snowbound train.",
                TotalCopies = 3, AvailableCopies = 3, PublishedDate = new DateTime(1934, 1, 1, 0, 0, 0, DateTimeKind.Utc), Language = "English",
                BookCategories = [new() { CategoryId = mystery.Id }, new() { CategoryId = fiction.Id }]
            },
            new()
            {
                Title = "A Brief History of Time", ISBN = "978-0553380163", AuthorId = hawking.Id,
                Description = "A landmark book exploring cosmology, black holes, and the nature of time.",
                TotalCopies = 4, AvailableCopies = 4, PublishedDate = new DateTime(1988, 4, 1, 0, 0, 0, DateTimeKind.Utc), Language = "English",
                BookCategories = [new() { CategoryId = science.Id }]
            }
        };

        context.Books.AddRange(books);
        await context.SaveChangesAsync();
    }
}
