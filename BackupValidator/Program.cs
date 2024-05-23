// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Text.Json;
using BackupValidator;
using BackupValidator.Infrastructure.Handler;
using BackupValidator.Infrastructure.Query;
using BackupValidator.Models;

var connectionString = "Server=localhost;Database=BackupDbTest;Trusted_Connection=True;TrustServerCertificate=True;";
var validations = new List<RowValidation>();
var validationResult = new ValidationResult();

var stopWatch = new Stopwatch();
stopWatch.Start();

// Creating validations
IValidationsFromPaginationQueryHandler validationsFromPaginationQueryHandler =
    new SqlValidationsFromPaginationQueryHandler();
ITableHasingFromIdRangeQueryHandler hasingFromIdRangeQueryHandler = new SqlTableHasingFromIdRangeQueryHandler();

int currentTakeCount = 0;
int page = 0;
int take = 1000;

do
{
    var result = (await validationsFromPaginationQueryHandler.Handle(new ValidationsFromPaginationQuery()
    {
        ConnectionString = connectionString,
        EntryPoint = "TestTable",
        IdProperty = "Id",
        SkipCount = page * take,
        TakeCount = take
    })).ToList();
    validations.AddRange(result);
    currentTakeCount = result.Count();
    page++;
} while (currentTakeCount > 0);

validations.Add(new RowValidation()
{
    EntryPoint = "TestTable",
    Hash = "123",
    Id = "5"
});

// Validating db
var chunks = validations.Chunk(1000);

foreach (var chunk in chunks)
{
    var resultMapping = (await hasingFromIdRangeQueryHandler.Handle(new ValidationsFromIdRangeQuery()
    {
        ConnectionString = connectionString,
        EntryPoint = "TestTable",
        IdProperty = "Id",
        IdRange = chunk.Select(x => x.Id).ToArray()
    })).ToDictionary(row => row.Id, row => row.Hash);

    foreach (var validation in chunk)
    {
        validationResult.ValidatedRowCount++;
        if (!resultMapping.ContainsKey(validation.Id))
        {
            validationResult.Anomalies.Add(new Anomaly
            {
                Id = validation.Id,
                Type = AnomalyType.Deleted
            });
            continue;
        }

        if (validation.Hash != resultMapping[validation.Id])
        {
            validationResult.Anomalies.Add(new Anomaly()
            {
                Id = validation.Id,
                Type = AnomalyType.Modified
            });
            continue;
        }
    }
}


Console.WriteLine(JsonSerializer.Serialize(validationResult));

stopWatch.Stop();

Console.WriteLine($"Time elapsed {stopWatch.Elapsed}");

Console.ReadLine();