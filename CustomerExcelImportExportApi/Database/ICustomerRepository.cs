namespace CustomerExcelImportExportApi;

using CustomerExcelImportExport.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICustomerRepository
{
    Task AddCustomerAsync(Customer customer);
    Task AddCustomersAsync(List<Customer> customers);
    Task<Customer> GetCustomerByIdAsync(int customerId);
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task UpdateCustomerAsync(Customer customer);
    Task DeleteCustomerAsync(int customerId);
    Task<List<Customer>> GetCustomersAsync(int pageNumber, int pageSize);
    Task<int> GetTotalCustomerCountAsync();

}