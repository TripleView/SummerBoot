using SummerBoot.Repository;

namespace SummerBoot.Performance.Test
{
    [AutoRepository1]
    public interface INullableTableRepository : IBaseRepository<NullableTable>
    {

    }
}