namespace DatabaseParser.ExpressionParser
{
    public interface IDbExecute
    {
        void Insert<T>(T insertEntity);
        void Update<T>(T updateEntity);

        void Delete<T>(T deleteEntity);
    }
}