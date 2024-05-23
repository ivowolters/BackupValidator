using BackupValidator.Infrastructure.Query;
using BackupValidator.Models;

namespace BackupValidator.Infrastructure.Handler;

public interface ITableHasingFromIdRangeQueryHandler
{
    Task<IEnumerable<RowValidation>> Handle(ValidationsFromIdRangeQuery query);
}