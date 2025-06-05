using System.Collections.Generic;
using System.Threading.Tasks;

namespace SummerBoot.Repository;

public interface IEntityClassHandler
{
    void ProcessingEntity(IBaseEntity entity, bool isUpdate = false);
    Task ProcessingEntityAsync(IBaseEntity entity, bool isUpdate = false);
}

public class DefaultEntityClassHandler : IEntityClassHandler
{
    public void ProcessingEntity(IBaseEntity entity, bool isUpdate = false)
    {
        
    }

    public async Task ProcessingEntityAsync(IBaseEntity entity, bool isUpdate = false)
    {

    }
}