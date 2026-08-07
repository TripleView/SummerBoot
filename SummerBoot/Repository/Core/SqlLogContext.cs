namespace SummerBoot.Repository.Core;

public class SqlLogContext
{
    public string Sql { get; set; }
    public string CountSql { get; set; }
    public DynamicParameters Parameters { get; set; }
}