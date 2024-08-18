using CustomerExcelImportExport.Shared;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Globalization;

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

        var response = new PaginatedResponse<Customer>
        {
            Items = customers ?? new List<Customer>(),  // Ensure `Items` is never null
            TotalCount = totalCustomers
        };

        return Ok(response);
    }


    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var customers = await _customerRepository.GetAllCustomersAsync(); 

        var excelFile = GenerateExcelFile(customers); 

        return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "customers.xlsx");
    }

    private byte[] GenerateExcelFile(IEnumerable<Customer> customers)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Customers");

        // Add headers
        worksheet.Cells[1, 1].Value = "Name";
        worksheet.Cells[1, 2].Value = "Email";
        worksheet.Cells[1, 3].Value = "Phone Number";
        worksheet.Cells[1, 4].Value = "Address";
        worksheet.Cells[1, 5].Value = "Date of Birth";
        worksheet.Cells[1, 6].Value = "Gender";
        worksheet.Cells[1, 7].Value = "Occupation";
        worksheet.Cells[1, 8].Value = "Created Date";
        worksheet.Cells[1, 9].Value = "Last Updated Date";
        worksheet.Cells[1, 10].Value = "Is Active";

        // Add data
        var row = 2;
        foreach (var customer in customers)
        {
            worksheet.Cells[row, 1].Value = customer.Name;
            worksheet.Cells[row, 2].Value = customer.Email;
            worksheet.Cells[row, 3].Value = customer.PhoneNumber;
            worksheet.Cells[row, 4].Value = customer.Address;
            worksheet.Cells[row, 5].Value = customer.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            worksheet.Cells[row, 6].Value = customer.Gender;
            worksheet.Cells[row, 7].Value = customer.Occupation;
            worksheet.Cells[row, 8].Value = customer.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            worksheet.Cells[row, 9].Value = customer.LastUpdatedDate?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            worksheet.Cells[row, 10].Value = customer.IsActive;
            row++;
        }

        return package.GetAsByteArray();
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportCustomers(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                return BadRequest("Invalid Excel file.");
            }

            var customers = new List<Customer>();

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                var customer = new Customer
                {
                    Name = worksheet.Cells[row, 1].Text,
                    Email = worksheet.Cells[row, 2].Text,
                    PhoneNumber = worksheet.Cells[row, 3].Text,
                    Address = worksheet.Cells[row, 4].Text,
                    DateOfBirth = DateTime.TryParseExact(worksheet.Cells[row, 5].Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirth) ? dateOfBirth : DateTime.MinValue,
                    Gender = worksheet.Cells[row, 6].Text,
                    Occupation = worksheet.Cells[row, 7].Text,
                    CreatedDate = DateTime.TryParseExact(worksheet.Cells[row, 8].Text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var createdDate) ? createdDate : DateTime.MinValue,
                    LastUpdatedDate = DateTime.TryParseExact(worksheet.Cells[row, 9].Text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var lastUpdatedDate) ? lastUpdatedDate : null,
                    IsActive = bool.TryParse(worksheet.Cells[row, 10].Text, out var isActive) ? isActive : false,
                };

                customers.Add(customer);
            }

            await _customerRepository.AddCustomersAsync(customers);

            return Ok("Customers imported successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _customerRepository.GetCustomerByIdAsync(id);
        if (customer == null)
        {
            return NotFound("Customer not found.");
        }

        await _customerRepository.DeleteCustomerAsync(customer.CustomerID);

        return Ok("Customer deleted successfully.");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAllCustomers()
    {
        await _customerRepository.DeleteAllCustomersAsync();
        return Ok("All customers deleted successfully.");
    }

}
