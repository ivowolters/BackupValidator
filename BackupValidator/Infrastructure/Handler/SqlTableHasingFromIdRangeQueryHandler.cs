using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BackupValidator.Infrastructure.Query;
using BackupValidator.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BackupValidator.Infrastructure.Handler;

public class SqlTableHasingFromIdRangeQueryHandler : ITableHasingFromIdRangeQueryHandler
{
    private readonly HashAlgorithm _hashAlgorithm = SHA256.Create();

    public async Task<IEnumerable<RowValidation>> Handle(ValidationsFromIdRangeQuery query)
    {
        using var connection = new SqlConnection(query.ConnectionString);

        var result = await connection.QueryAsync(
            $"SELECT * FROM {query.EntryPoint} WHERE Id IN @ids",
            new { ids = query.IdRange }
        );

        return result
            .Select(x => new RowValidation()
            {
                EntryPoint = query.EntryPoint,
                Id = (x as IDictionary<string, object>)![query.IdProperty].ToString(),
                Hash = Encoding.Default.GetString(_hashAlgorithm.ComputeHash(JsonSerializer.SerializeToUtf8Bytes(x)))
            });
    }
}