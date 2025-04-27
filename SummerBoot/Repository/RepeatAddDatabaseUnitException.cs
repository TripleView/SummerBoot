using System;

namespace SummerBoot.Repository;

/// <summary>
/// Repeatedly add database unit
/// 重复添加数据库单元
/// </summary>
[Serializable]
public class RepeatAddDatabaseUnitException:Exception
{
    public RepeatAddDatabaseUnitException():base("Repeatedly add database unit")
    {
        
    }
}