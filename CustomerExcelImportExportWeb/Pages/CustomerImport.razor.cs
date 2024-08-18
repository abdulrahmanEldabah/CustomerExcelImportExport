using CustomerExcelImportExport.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.JSInterop;
using System;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using static System.Net.WebRequestMethods;

namespace CustomerExcelImportExportWeb;

public partial class CustomerImport
{
    [Inject] private IJSRuntime JSRuntime { get; set; }

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
    private async Task ExportToExcel()
    {
        try
        {
            var url = "api/Customer/export"; // The correct API endpoint

            // Request the Excel file from the server
            var response = await HttpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                var fileName = "customers.xlsx";

                // Use JavaScript Interop to trigger the download
                await JSRuntime.InvokeVoidAsync("downloadFile", content, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            else
            {
                Console.WriteLine("Error exporting data.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;

        if (file != null)
        {
            using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            // Send the file to the server for processing
            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(memoryStream.ToArray()), "file", file.Name);

            var response = await HttpClient.PostAsync("api/Customer/import", content);

            if (response.IsSuccessStatusCode)
            {
                LoadCustomer();
            }
            else
            {
            }
        }
    }


}
