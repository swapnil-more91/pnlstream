namespace PnLStream.Common.Contracts;

public class ValidationResultEvent
{
    public PnlRecordEvent PnlRecordRecordEvent { get; set; }

    public List<string> Errors { get; } = new();

    public bool IsValid => Errors.Count == 0;
}
     
