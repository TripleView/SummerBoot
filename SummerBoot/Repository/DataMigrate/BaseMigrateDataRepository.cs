using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SummerBoot.Core;
using SummerBoot.Repository.DataMigrate;
using SummerBoot.Repository.ExpressionParser.Parser;

namespace SummerBoot.Repository.DataMigrate;

public abstract class BaseMigrateDataRepository : CustomBaseRepository<BaseEntity>, IDataMigrateRepository
{
    private readonly IDbFactory dbFactory;
    private readonly IUnitOfWork unitOfWork;
    /// <summary>
    /// Left Qualifier，Delimited Identifiers;左限定符,用来引用标识符（如表名、列名等）
    /// </summary>
    protected string leftQuote;
    /// <summary>
    /// Right Qualifier;右限定符
    /// </summary>
    protected string rightQuote;
    public BaseMigrateDataRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        this.dbFactory = unitOfWork.DbFactory;
        this.unitOfWork = unitOfWork;
    }
  
    public abstract Task MigratingDataWithAutoIncrementPrimaryKeyAsync<T>(Expression<Func<T, object>> selectKey, Func<Task> migrateAction);
    public abstract Task DisableForeignKeysAsync(List<string> tableNames);
    public abstract Task EnableForeignKeysAsync(List<string> tableNames);
}
