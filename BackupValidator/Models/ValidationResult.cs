namespace BackupValidator.Models;

public class ValidationResult
{
    public List<Anomaly> Anomalies { get; init; } = new();

    public int ValidatedRowCount { get; set; }

    public bool IsSuccess => Anomalies.Count == 0;
}