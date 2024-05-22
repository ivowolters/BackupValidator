namespace BackupValidator.Infrastructure.Query;

public class ValidationsFromPaginationQuery
{
    public string ConnectionString { get; set; }
    
    public string EntryPoint { get; set; }
    
    public string IdProperty { get; set; }
    
    public int SkipCount { get; set; }
    
    public int TakeCount { get; set; }
}