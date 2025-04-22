using System.Collections.Generic;
using System.Threading.Tasks;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Test.Oracle.Models;

namespace SummerBoot.Test.Oracle.Repository
{
    [OracleAutoRepository]
    public interface IGuidModelRepository : IBaseRepository<GuidModel>
    {
        
    }

  
    public interface IGuidModelForListParameterRepository : IBaseRepository<GuidModel>
    {
        Task<List<GuidModel>> GetListByNames(List<string> names);
    }

    [OracleManualRepository(typeof(IGuidModelForListParameterRepository))]
    public class GuidModelForListParameterRepository : CustomBaseRepository<GuidModel>,
        IGuidModelForListParameterRepository
    {
        public GuidModelForListParameterRepository(IUnitOfWork1 uow) :base(uow)
        {
            
        }

        public async Task<List<GuidModel>> GetListByNames(List<string> names)
        {
            this.OpenDb();
            var r = await this.QueryListAsync<GuidModel>("select * from GuidModel where Name in :names",
                new { names = names });
            this.CloseDb();
            return r;
        }

    }
}