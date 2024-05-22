using BackupValidator.Infrastructure.Query;
using BackupValidator.Models;

namespace BackupValidator.Infrastructure.Handler;

public interface IValidationsFromPaginationQueryHandler
{
    Task<IEnumerable<RowValidation>> Handle(ValidationsFromPaginationQuery query);
}