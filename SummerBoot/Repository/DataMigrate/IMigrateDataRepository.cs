using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SummerBoot.Repository.DataMigrate;

public interface IDataMigrateRepository
{
    /// <summary>
    /// 当前仅支持oracle，单表有主键的情况下进行迁移，先移除自增序列，插入数据，新建newId字段,设置newId值为为id,删除id，重命名newid为id，恢复id自增序列
    /// </summary>
    /// <returns></returns>
    Task MigratingDataWithAutoIncrementPrimaryKeyAsync<T>(Expression<Func<T, object>> selectKey, Func<Task> migrateAction);
    /// <summary>
    /// Disable foreign keys;禁用外键
    /// </summary>
    /// <param name="tableNames">Table name list;表名列表</param>
    /// <returns></returns>
    Task DisableForeignKeysAsync(List<string> tableNames);
    /// <summary>
    /// enable foreign keys;启用外键
    /// </summary>
    /// <param name="tableNames">Table name list;表名列表</param>
    /// <returns></returns>
    Task EnableForeignKeysAsync(List<string> tableNames);
}

public interface IDataMigrateRepository2: IDataMigrateRepository{}
public interface IDataMigrateRepository3 : IDataMigrateRepository { }
public interface IDataMigrateRepository4 : IDataMigrateRepository { }
public interface IDataMigrateRepository5 : IDataMigrateRepository { }
public interface IDataMigrateRepository6 : IDataMigrateRepository { }
public interface IDataMigrateRepository7 : IDataMigrateRepository { }
public interface IDataMigrateRepository8 : IDataMigrateRepository { }
public interface IDataMigrateRepository9 : IDataMigrateRepository { }
