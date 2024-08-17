using CustomerExcelImportExport.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CustomerExcelImportExportApi;

[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ICustomerRepository _customerRepository;

    public CustomerController(HttpClient httpClient, ICustomerRepository databaseService)
    {
        _httpClient = httpClient;
        _customerRepository = databaseService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
    {
        if (customer == null || !ModelState.IsValid)
        {
            return BadRequest("Invalid customer data.");
        }
        await _customerRepository.AddCustomerAsync(customer);

        return Ok("Customer created successfully.");
    }
    [HttpGet]
    public async Task<IActionResult> GetCustomers(int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("Invalid pagination parameters.");
        }

        var customers = await _customerRepository.GetCustomersAsync(pageNumber, pageSize);
        var totalCustomers = await _customerRepository.GetTotalCustomerCountAsync();

        if (customers == null || customers.Count == 0)
        {
            return NotFound("No customers found.");
        }

        var response = new PaginatedResponse<Customer>
        {
            Items = customers,
            TotalCount = totalCustomers
        };

        return Ok(response);
    }

}


