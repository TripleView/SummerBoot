using System;
using System.Threading.Tasks;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Test.Oracle.Models;

namespace SummerBoot.Test.Oracle.Repository
{
    [OracleAutoRepository]
    public interface INullableTable2Repository : IBaseRepository<NullableTable2>
    {

    }

    [OracleAutoRepository]
    public interface INullableTableRepository : IBaseRepository<NullableTable>
    {

    }

    public interface INullableTableManualRepository : IBaseRepository<NullableTable>
    {
        Task<Guid?> GetGuid();
        Task<Guid?> GetGuidWithNullResult();
    }

    [OracleManualRepositoryAttribute(typeof(INullableTableManualRepository))]
    public class NullableTableManualRepository : CustomBaseRepository<NullableTable>, INullableTableManualRepository
    {
        private readonly IUnitOfWork1 unitOfWork1;

        public NullableTableManualRepository(IUnitOfWork1 uow) : base(uow)
        {
            this.unitOfWork1 = uow;
        }

        public async Task<Guid?> GetGuid()
        {
            this.OpenDb();
            var result = await this.QueryFirstOrDefaultAsync<Guid?>("select Guid2 from NULLABLETABLE");
            this.CloseDb();
            return result;
        }

        public async Task<Guid?> GetGuidWithNullResult()
        {
            this.OpenDb();
            var result = await this.QueryFirstOrDefaultAsync<Guid?>("select Guid2 from NULLABLETABLE where 1=2");
            this.CloseDb();
            return result;
        }
    }

    [OracleAutoRepository]
    public interface INotNullableTableRepository : IBaseRepository<NotNullableTable>
    {

    }
}