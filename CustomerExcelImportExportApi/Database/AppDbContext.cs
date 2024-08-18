namespace CustomerExcelImportExportApi;

using CustomerExcelImportExport.Shared;

// AppDbContext.cs
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Customer entity
        modelBuilder.Entity<Customer>(entity =>
        {
            // Set CustomerID as the primary key
            entity.HasKey(c => c.CustomerID);

            // Ensure CustomerID is auto-incremented
            entity.Property(c => c.CustomerID)
                .ValueGeneratedOnAdd();

            // Configure Name property
            entity.Property(c => c.Name)
                .IsRequired() // Make the Name property required
                .HasMaxLength(100); // Set maximum length for Name

            // Configure Email property
            entity.Property(c => c.Email)
                .IsRequired() // Make the Email property required
                .HasMaxLength(100); // Set maximum length for Email
            entity.HasIndex(c => c.Email)
                .IsUnique(); // Ensure Email is unique

            // Configure PhoneNumber property
            entity.Property(c => c.PhoneNumber)
                .HasMaxLength(15); // Set maximum length for PhoneNumber

            // Configure Address property
            entity.Property(c => c.Address)
                .HasMaxLength(200); // Set maximum length for Address

            // Configure DateOfBirth property
            entity.Property(c => c.DateOfBirth)
                .HasColumnType("date"); // Set column type to date

            // Configure Gender property
            entity.Property(c => c.Gender)
                .HasMaxLength(10); // Set maximum length for Gender

            // Configure Occupation property
            entity.Property(c => c.Occupation)
                .HasMaxLength(50); // Set maximum length for Occupation

            // Configure CreatedDate property
            entity.Property(c => c.CreatedDate)
                .IsRequired() // Ensure CreatedDate is always set
                .HasDefaultValueSql("GETDATE()"); // Set default value to current date

            // Configure LastUpdatedDate property
            entity.Property(c => c.LastUpdatedDate)
                .HasColumnType("datetime");

            // Configure IsActive property
            entity.Property(c => c.IsActive)
                .IsRequired() // Ensure IsActive is always set
                .HasDefaultValue(true); // Set default value to true
        });
    }
}
