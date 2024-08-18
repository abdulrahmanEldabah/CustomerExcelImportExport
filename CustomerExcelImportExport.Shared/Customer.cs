namespace CustomerExcelImportExport.Shared;

public class Customer
{
    public int CustomerID { get; set; }  // Unique identifier for the customer
    public string Name { get; set; }  // Full name of the customer
    public string Email { get; set; }  // Email address of the customer
    public string PhoneNumber { get; set; }  // Contact phone number
    public string Address { get; set; }  // Physical address of the customer
    public DateTime DateOfBirth { get; set; }  // Date of birth of the customer
    public string Gender { get; set; }  // Gender of the customer
    public string Occupation { get; set; }  // Occupation or job title of the customer
    public DateTime CreatedDate { get; set; } = DateTime.Now;  // The date the customer was added to the system
    public DateTime? LastUpdatedDate { get; set; }  // The last date the customer's information was updated
    public bool IsActive { get; set; } = true;  // Status to indicate if the customer is active or not
}
