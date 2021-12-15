using System.Collections.Generic;

namespace DatabaseParser.ExpressionParser
{
    public interface IDbExecute
    {
        DbQueryResult Insert<T>(T insertEntity);
        DbQueryResult Update<T>(T updateEntity);

        DbQueryResult Delete<T>(T deleteEntity);

        DbQueryResult GetAll<T>();

        DbQueryResult Get<T>(dynamic id);

        PageQueryResult GetByPage(string sql);
    }
}