using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;



var dbContextOptionsBuilder = new DbContextOptionsBuilder();
dbContextOptionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS; Database=EfCoreGroupByNullable; Integrated Security=True; TrustServerCertificate=True");

var applicationDbContext = new ApplicationDbContext(dbContextOptionsBuilder.Options);
await applicationDbContext.Database.EnsureDeletedAsync();
await applicationDbContext.Database.EnsureCreatedAsync();



applicationDbContext.Categories.AddRange(
    new Category("1 (Group NULL)", null, DateTime.MaxValue),
    new Category("2 (Group NULL)", null, DateTime.MaxValue),
    new Category("3 (Group 1)", 1, DateTime.MaxValue),
    new Category("4 (Group 2)", 2, DateTime.MaxValue),
    new Category("5 (Group 2)", 2, DateTime.MaxValue));

await applicationDbContext.SaveChangesAsync();



var query = applicationDbContext.Categories
    .GroupBy(x => x.GroupId)
    .Select(x => x.OrderByDescending(x => x.CreationDateTime).First());

var sql = query.ToQueryString();

var result = await query.ToListAsync();



// Workaround:
var query2 = applicationDbContext.Categories
    .GroupBy(x => x.GroupId ?? 0)
    .Select(x => x.OrderByDescending(x => x.CreationDateTime).First());

var sql2 = query2.ToQueryString();

var result2 = await query2.ToListAsync();



Console.Read();



public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLoggerFactory(LoggerFactory.Create(x => x
                .AddConsole()
                .AddFilter(y => y >= LogLevel.Debug)))
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();

        base.OnConfiguring(optionsBuilder);
    }
}

public class Category
{
    public Category(string name, int? groupId, DateTime creationDateTime)
    {
        Name = name;
        GroupId = groupId;
        CreationDateTime = creationDateTime;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public int? GroupId { get; private set; }
    public DateTime CreationDateTime { get; private set; }

    public override string ToString() => Name;
}