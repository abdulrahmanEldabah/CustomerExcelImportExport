using CustomerExcelImportExport.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using System;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace CustomerExcelImportExportWeb;

public partial class CustomerImport
{
    private Customer customer = new Customer();
    private Customer submittedCustomer;
    PaginationState pagination = new PaginationState { ItemsPerPage = 10 };
    GridItemsProvider<Customer>? customers;

    protected override async Task OnInitializedAsync()
    {
        LoadCustomer();
    }

    private void LoadCustomer()
    {
        customers = async req =>
        {
            // Construct the URL for the API call with pagination parameters
            var url = NavManager.GetUriWithQueryParameters(
                "api/customer",
                new Dictionary<string, object?>
                {
                { "pageNumber", (req.StartIndex / req.Count) + 1 }, // Calculate page number
                { "pageSize", req.Count }, // Number of items per page
                });

            // Make the API call
            var response = await HttpClient.GetFromJsonAsync<PaginatedResponse<Customer>>(url, req.CancellationToken);

            if (response == null)
            {
                return GridItemsProviderResult.From(
                    items: Array.Empty<Customer>(),
                    totalItemCount: 0);
            }

            // Return the items and total count
            return GridItemsProviderResult.From(
                items: response.Items,
                totalItemCount: response.TotalCount);
        };
    }

    private async Task HandleValidSubmit()
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/Customer", customer);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Customer created successfully.");

                customer = new Customer();
                LoadCustomer();
            }
            else
            {
                // Handle error (e.g., show an error message)
                Console.WriteLine("Error creating customer.");
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., network errors)
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
