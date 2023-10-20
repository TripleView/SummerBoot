namespace SummerBoot.Repository.ExpressionParser.Parser.MultiQuery
{
    public interface IJoinCondition    {

    }
    public class JoinCondition<Table1, Table2>: IJoinCondition
    {
        public Table1 T1 { get; set; }
        public Table2 T2 { get; set; }

    }

    public class JoinCondition<Table1, Table2, Table3>: IJoinCondition
    {
        public Table1 T1 { get; set; }
        public Table2 T2 { get; set; }
        public Table3 T3 { get; set; }

    }

    public class JoinCondition<Table1, Table2, Table3, Table4>: JoinCondition<Table1, Table2, Table3>
    {
        public Table4 T4 { get; set; }
    }
}