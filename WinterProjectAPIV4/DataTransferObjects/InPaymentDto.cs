namespace WinterProjectAPIV4.DataTransferObjects
{
    public class InPaymentDto
    {
        public int? TransactionID { get; set; }
        public int? UserGroupId { get; set; }

        public double? Amount { get; set; }
    }
}
