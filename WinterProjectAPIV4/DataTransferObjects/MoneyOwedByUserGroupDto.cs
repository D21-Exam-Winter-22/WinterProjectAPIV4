namespace WinterProjectAPIV4.DataTransferObjects
{
    public class MoneyOwedByUserGroupDto
    {
        public int UserID { get; set; }
        public int GroupID { get; set; }
        public double FinalAmountOwed { get; set; }

        public double AmountAlreadyPaid { get; set; }
        public double AmountPaidDuringGroup { get; set; }
    }
}
