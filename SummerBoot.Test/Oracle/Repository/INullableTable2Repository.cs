using SummerBoot.Repository;
using SummerBoot.Test.Oracle.Models;

namespace SummerBoot.Test.Oracle.Repository
{
    [AutoRepository]
    public interface INullableTable2Repository: IBaseRepository<NullableTable2>
    {
        
    }

    [AutoRepository]
    public interface INullableTableRepository : IBaseRepository<NullableTable>
    {

    }

    [AutoRepository]
    public interface INotNullableTableRepository : IBaseRepository<NotNullableTable>
    {

    }
}