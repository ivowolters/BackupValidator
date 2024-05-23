namespace BackupValidator.Infrastructure.Query;

public class ValidationsFromIdRangeQuery
{
    public string ConnectionString { get; init; }
    
    public string EntryPoint { get; init; }
    
    public string IdProperty { get; init; }
    
    public string[] IdRange { get; init; }
}