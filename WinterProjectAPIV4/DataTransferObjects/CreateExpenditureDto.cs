namespace WinterProjectAPIV4.DataTransferObjects
{
    public class CreateExpenditureDto
    {
        public int UserID { get; set; }
        public int GroupID { get; set; }
        public double? Amount { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}
