namespace WinterProjectAPIV4.DataTransferObjects
{
    public class GetAllExpensesDto
    {
        public int? ExpenseId { get; set; }
        public double? Amount { get; set; }
        public int? UserId { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        
        public string? ExpenseName { get; set; }
        
        public string? ExpenseDescription { get; set; }
        
        public string? GroupDescription { get; set; }
    }
}
