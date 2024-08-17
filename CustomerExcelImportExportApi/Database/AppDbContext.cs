namespace CustomerExcelImportExportApi;

using CustomerExcelImportExport.Shared;

// AppDbContext.cs
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Customer entity
        modelBuilder.Entity<Customer>()
            .HasKey(c => c.CustomerID); // Set CustomerID as the primary key

        // Optional: Configure auto-increment behavior (if needed)
        modelBuilder.Entity<Customer>()
            .Property(c => c.CustomerID)
            .ValueGeneratedOnAdd(); // Ensure CustomerID is auto-incremented
    }
    public DbSet<Customer> Customers { get; set; } 
}
