using Npgsql;
using NpgsqlTypes;
using SummerBoot.Core;
using SummerBoot.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SummerBoot.Pgsql;

public class PgsqlDatabaseSpecificProvider : DefaultDatabaseSpecificProvider, IDatabaseSpecificProvider
{
    public PgsqlDatabaseSpecificProvider(IUnitOfWork uow) : base(uow)
    {
    }
    public override void FastBatchInsert<T>(List<T> list)
    {
        throw new System.NotImplementedException();
    }

    public override async Task FastBatchInsertAsync<T>(List<T> list)
    {
        using (var writer = await ((NpgsqlConnection)dbConnection).BeginBinaryImportAsync("COPY person (id, name, birthday) FROM STDIN (FORMAT BINARY)"))
        {
            foreach (var p in list)
            {
                //await writer.StartRowAsync();
                //await writer.WriteAsync(1, "123");
                //await writer.WriteAsync(p.Id, NpgsqlDbType.Integer);
                //await writer.WriteAsync(p.Name, NpgsqlDbType.Text);
                //await writer.WriteAsync(p.Birthday, NpgsqlDbType.Timestamp);
            }
            await writer.CompleteAsync();
        }
    }
}