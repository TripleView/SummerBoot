using SummerBoot.Repository;
using SummerBoot.Test.Oracle.Models;

namespace SummerBoot.Test.Oracle.Repository
{
    [OracleAutoRepository]
    public interface INullableTable2Repository: IBaseRepository<NullableTable2>
    {
        
    }

    [OracleAutoRepository]
    public interface INullableTableRepository : IBaseRepository<NullableTable>
    {

    }

    [OracleAutoRepository]
    public interface INotNullableTableRepository : IBaseRepository<NotNullableTable>
    {

    }
}