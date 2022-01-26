namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public interface IGetDbExecuteSql
    {
        DbQueryResult Insert<T>(T insertEntity);
        DbQueryResult Update<T>(T updateEntity);

        DbQueryResult Delete<T>(T deleteEntity);

        DbQueryResult GetAll<T>();

        DbQueryResult Get<T>(dynamic id);

        PageQueryResult GetByPage(string sql);
    }
}