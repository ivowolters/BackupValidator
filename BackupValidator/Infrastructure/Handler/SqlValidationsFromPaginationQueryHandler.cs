using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BackupValidator.Infrastructure.Query;
using BackupValidator.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BackupValidator.Infrastructure.Handler;

public class SqlValidationsFromPaginationQueryHandler : IValidationsFromPaginationQueryHandler
{
    private readonly HashAlgorithm _hashAlgorithm = SHA256.Create();

    public async Task<IEnumerable<RowValidation>> Handle(ValidationsFromPaginationQuery query)
    {
        var dbQuery = $"""
                       SELECT * FROM {query.EntryPoint}
                       ORDER BY {query.IdProperty}
                       OFFSET @skipCount ROWS FETCH FIRST @takeCount ROWS ONLY;
                       """;

        await using var connection = new SqlConnection(query.ConnectionString);

        var result = await connection.QueryAsync<dynamic>(dbQuery, new
        {
            skipCount = query.SkipCount,
            takeCount = query.TakeCount
        });

        return result.Select(row => new RowValidation()
        {
            EntryPoint = query.EntryPoint,
            Id = (row as IDictionary<string, object>)![query.IdProperty].ToString()!,
            Hash = Encoding.Default.GetString(
                _hashAlgorithm.ComputeHash((byte[])JsonSerializer.SerializeToUtf8Bytes(row)))
        });
    }
}