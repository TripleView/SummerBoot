using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SummerBoot.Repository.DataMigrate;

public interface IDataMigrateRepository
{
    /// <summary>
    /// 当前仅支持oracle，单表有主键的情况下进行迁移，先移除自增序列，插入数据，新建newId字段,设置newId值为为id,删除id，重命名newid为id，恢复id自增序列
    /// </summary>
    /// <returns></returns>
    Task MigratingDataWithAutoIncrementPrimaryKey<T>(Expression<Func<T, object>> selectKey, Func<Task> migrateAction);
}

public interface IDataMigrateRepository2: IDataMigrateRepository{}
public interface IDataMigrateRepository3 : IDataMigrateRepository { }
public interface IDataMigrateRepository4 : IDataMigrateRepository { }
public interface IDataMigrateRepository5 : IDataMigrateRepository { }
public interface IDataMigrateRepository6 : IDataMigrateRepository { }
public interface IDataMigrateRepository7 : IDataMigrateRepository { }
public interface IDataMigrateRepository8 : IDataMigrateRepository { }
public interface IDataMigrateRepository9 : IDataMigrateRepository { }
