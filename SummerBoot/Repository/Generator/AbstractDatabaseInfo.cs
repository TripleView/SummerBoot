using SummerBoot.Repository.Generator.Dto;

namespace SummerBoot.Repository.Generator
{
    public abstract class AbstractDatabaseInfo : IDatabaseInfo
    {
        public AbstractDatabaseInfo(string leftQuote, string rightQuote,DatabaseUnit databaseUnit)
        {
            this.leftQuote = leftQuote;
            this.rightQuote = rightQuote;
            this.databaseUnit = databaseUnit;
        }

        protected string leftQuote;
        protected string rightQuote;
        protected DatabaseUnit databaseUnit;

        protected string BoxColumnName(string columnName)
        {
            if (columnName == "*")
            {
                return columnName;
            }

            if (databaseUnit.ColumnNameMapping != null)
            {
                columnName = databaseUnit.ColumnNameMapping(columnName);
            }

            return CombineQuoteAndName(columnName);
        }

        protected string BoxTableName(string tableName)
        {
            if (databaseUnit.TableNameMapping != null)
            {
                tableName = databaseUnit.TableNameMapping(tableName);
            }
            return CombineQuoteAndName(tableName);
        }

        private string CombineQuoteAndName(string name)
        {
            return leftQuote + name + rightQuote;
        }
        public abstract string CreatePrimaryKey(string schema, string tableName, DatabaseFieldInfoDto fieldInfo);

        public abstract GenerateDatabaseSqlResult CreateTable(DatabaseTableInfoDto tableInfo);

        public abstract string CreateTableDescription(string schema, string tableName, string description);

        public abstract string CreateTableField(string schema, string tableName, DatabaseFieldInfoDto fieldInfo);

        public abstract string CreateTableFieldDescription(string schema, string tableName,
            DatabaseFieldInfoDto fieldInfo);

        public abstract string GetDefaultSchema(string schema);

        public abstract string GetSchemaTableName(string schema, string tableName);

        public abstract DatabaseTableInfoDto GetTableInfoByName(string schema, string tableName);

        public abstract string UpdateTableDescription(string schema, string tableName, string description);
    }
}
