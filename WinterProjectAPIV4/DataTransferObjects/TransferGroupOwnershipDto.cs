namespace WinterProjectAPIV4.DataTransferObjects;

public class TransferGroupOwnershipDto
{
    public int GroupID { get; set; }
    public int PreviousOwnerUserID { get; set; }
    public int NewOwnerUserID { get; set; }
}