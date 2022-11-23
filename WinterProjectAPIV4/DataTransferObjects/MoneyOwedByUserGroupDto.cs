namespace WinterProjectAPIV4.DataTransferObjects
{
    public class MoneyOwedByUserGroupDto
    {
        
        public int UserID { get; set; }
        public int GroupID { get; set; }
        public double FinalAmountOwed { get; set; }

        public double AmountAlreadyPaid { get; set; }
        public double AmountPaidDuringGroup { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
    }
}
