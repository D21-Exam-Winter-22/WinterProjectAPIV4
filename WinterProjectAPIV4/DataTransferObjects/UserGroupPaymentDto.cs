namespace WinterProjectAPIV4.DataTransferObjects
{
    public class UserGroupPaymentDto
    {
        public int? TransactionID { get; set; }
        public double? PaidAmount { get; set; }
        public int? UserGroupID { get; set; }
        public int? GroupID { get; set; }
        public string? GroupName { get; set; }
        public string? GroupDescription { get; set; }
        public int? UserID { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        
        
    }
}
