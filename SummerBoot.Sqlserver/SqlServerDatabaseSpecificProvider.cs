using System.Collections.Generic;
using System.Threading.Tasks;
using SummerBoot.Core;
using SummerBoot.Repository;

namespace SummerBoot.SqlServer;

public class SqlServerDatabaseSpecificProvider : DefaultDatabaseSpecificProvider, IDatabaseSpecificProvider
{
    public SqlServerDatabaseSpecificProvider(IUnitOfWork uow) : base(uow)
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