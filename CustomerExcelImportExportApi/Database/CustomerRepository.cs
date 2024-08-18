namespace CustomerExcelImportExportApi;

using CustomerExcelImportExport.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _dbContext;

    public CustomerRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Create a new customer
    public async Task AddCustomerAsync(Customer customer)
    {
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();
    }
    public async Task AddCustomersAsync(List<Customer> customers)
    {
        _dbContext.Customers.AddRange(customers);
        await _dbContext.SaveChangesAsync();
    }

    // Retrieve a customer by ID
    public async Task<Customer> GetCustomerByIdAsync(int customerId)
    {
        return await _dbContext.Customers.FindAsync(customerId);
    }

    // Retrieve all customers
    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _dbContext.Customers.ToListAsync();
    }

    // Update an existing customer
    public async Task UpdateCustomerAsync(Customer customer)
    {
        _dbContext.Customers.Update(customer);
        await _dbContext.SaveChangesAsync();
    }

    // Delete a customer by ID
    public async Task DeleteCustomerAsync(int customerId)
    {
        var customer = await GetCustomerByIdAsync(customerId);
        if (customer != null)
        {
            _dbContext.Customers.Remove(customer);
            await _dbContext.SaveChangesAsync();
        }
    }
    public async Task<List<Customer>> GetCustomersAsync(int pageNumber, int pageSize)
    {
        return await _dbContext.Customers
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    public async Task<int> GetTotalCustomerCountAsync()
    {
        return await _dbContext.Customers.CountAsync();
    }

}
