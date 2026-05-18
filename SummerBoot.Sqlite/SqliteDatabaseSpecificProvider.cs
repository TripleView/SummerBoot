using SummerBoot.Core;
using SummerBoot.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SummerBoot.Sqlite;

public class SqliteDatabaseSpecificProvider : DefaultDatabaseSpecificProvider, IDatabaseSpecificProvider
{
    public SqliteDatabaseSpecificProvider(IUnitOfWork uow) : base(uow)
    {
    }
    public override void FastBatchInsert<T>(List<T> list)
    {
        throw new System.NotImplementedException();
    }

    public override async Task FastBatchInsertAsync<T>(List<T> list)
    {
       
    }
}