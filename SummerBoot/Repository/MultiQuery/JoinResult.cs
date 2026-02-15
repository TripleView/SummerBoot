using System.Linq;
using SummerBoot.Repository.ExpressionParser.Parser;

namespace SummerBoot.Repository.MultiQuery;

public class JoinResult<T1, T2> 
{
    public IQueryable<JoinCondition<T1, T2>> Repository { get; set; }
}

public class JoinOrderByResult<T1, T2> : JoinResult<T1, T2>
{

}

public class JoinResult<T1, T2, T3>
{
    public IQueryable<JoinCondition<T1, T2, T3>> Repository { get; set; }
}

public class JoinOrderByResult<T1, T2, T3> : JoinResult<T1, T2, T3>
{

}

public class JoinResult<T1, T2, T3, T4>
{
}

public class JoinResult<T1, T2, T3, T4, T5> 
{
}

public class JoinResult<T1, T2, T3, T4, T5, T6>
{
}