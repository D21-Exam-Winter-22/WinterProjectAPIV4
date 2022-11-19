namespace WinterProjectAPIV4.DataTransferObjects;

public class UserInPaymentsDto
{
    public int? TransactionID { get; set; }
    public double? Amount { get; set; }
    public int? UserGroupID { get; set; }
    public int? GroupID { get; set; }
    public int? UserID { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}