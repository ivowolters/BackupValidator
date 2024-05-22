// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BackupValidator.Infrastructure.Handler;
using BackupValidator.Infrastructure.Query;
using BackupValidator.Models;
using Dapper;
using Microsoft.Data.SqlClient;

var connectionString = "Server=localhost;Database=BackupDbTest;Trusted_Connection=True;TrustServerCertificate=True;";
var validations = new List<RowValidation>();
var validationResult = new ValidationResult();
var connection = new SqlConnection(connectionString);

var stopWatch = new Stopwatch();
stopWatch.Start();

// Creating validations

IValidationsFromPaginationQueryHandler handler = new SqlValidationsFromPaginationQueryHandler();

int currentTakeCount = 0;
int page = 0;
int take = 1000;

do
{
    var result = (await handler.Handle(new ValidationsFromPaginationQuery()
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

Console.WriteLine(stopWatch.Elapsed);


// Validating db
using (SHA256 shaHashing = SHA256.Create())
{
    var chunks = validations.Chunk(1000);
    var query = "SELECT * FROM TestTable WHERE Id IN @ids";

    foreach (var chunk in chunks)
    {
        var result = connection.Query(query, new { ids = chunk.Select(validation => validation.Id) });
        var resultMapping = new Dictionary<string, string>();

        foreach (var row in result)
        {
            resultMapping.Add(
                (row as IDictionary<string, object>)!["Id"].ToString(),
                Encoding.Default.GetString(shaHashing.ComputeHash(JsonSerializer.SerializeToUtf8Bytes(row)))
            );
        }

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
}


Console.WriteLine(JsonSerializer.Serialize(validationResult));

stopWatch.Stop();

Console.WriteLine($"Time elapsed {stopWatch.Elapsed}");

Console.ReadLine();