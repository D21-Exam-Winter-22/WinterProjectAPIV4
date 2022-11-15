namespace WinterProjectAPIV4.DataTransferObjects
{
    public class UserGroupPaymentDto
    {
        public int TransactionID { get; set; }
        public int UserGroupID { get; set; }
        public double PaidAmount { get; set; }
        public int UserID { get; set; }
        public int GroupID { get; set; }
    }
}
