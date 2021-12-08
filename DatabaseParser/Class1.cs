using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DatabaseParser.Base;
using DatabaseParser.ExpressionParser;

namespace DatabaseParser
{
    //public class SimpleExpressionToSQL : ExpressionVisitor
    //{
    //    /*
    //     * Original By Ryan Wright: https://stackoverflow.com/questions/7731905/how-to-convert-an-expression-tree-to-a-partial-sql-query
    //     */

    //    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //    private readonly List<string> _groupBy = new List<string>();

    //    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //    private readonly List<string> _orderBy = new List<string>();

    //    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //    private readonly List<SqlParameter> _parameters = new List<SqlParameter>();

    //    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //    private readonly List<string> _select = new List<string>();

    //    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //    private readonly List<string> _update = new List<string>();

    //    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //    private readonly List<string> _where = new List<string>();

    //    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //    private int? _skip;

    //    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //    private int? _take;

    //    public SimpleExpressionToSQL(IQueryable queryable)
    //    {
    //        if (queryable is null)
    //        {
    //            throw new ArgumentNullException(nameof(queryable));
    //        }

    //        Expression expression = queryable.Expression;
    //        Visit(expression);

    //    }

    //    public string CommandText =>string.Join("_", BuildSqlStatement());


    //    public string From => $"FROM [{TableName}]";

    //    public string GroupBy => _groupBy.Count == 0 ? null : "GROUP BY " + string.Join(", ", _groupBy);
    //    public bool IsDelete { get; private set; } = false;
    //    public bool IsDistinct { get; private set; }
    //    public string OrderBy => string.Join(" ",BuildOrderByStatement());
    //    public SqlParameter[] Parameters => _parameters.ToArray();
    //    public string Select => string.Join(" ", BuildSelectStatement());
    //    public int? Skip => _skip;
    //    public string TableName { get; private set; }
    //    public int? Take => _take;
    //    public string Update => "SET " + string.Join(", ",_update);

    //    public string Where => _where.Count == 0 ? null : "WHERE " + string.Join(" ",_where);

    //    public static implicit operator string(SimpleExpressionToSQL simpleExpression) => simpleExpression.ToString();

    //    public override string ToString() =>"";

    //    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    //    {
    //        _where.Add("(");
    //        Visit(binaryExpression.Left);

    //        switch (binaryExpression.NodeType)
    //        {
    //            case ExpressionType.And:
    //                _where.Add("AND");
    //                break;

    //            case ExpressionType.AndAlso:
    //                _where.Add("AND");
    //                break;

    //            case ExpressionType.Or:
    //            case ExpressionType.OrElse:
    //                _where.Add("OR");
    //                break;

    //            case ExpressionType.Equal:
    //                if (IsNullConstant(binaryExpression.Right))
    //                {
    //                    _where.Add("IS");
    //                }
    //                else
    //                {
    //                    _where.Add("=");
    //                }
    //                break;

    //            case ExpressionType.NotEqual:
    //                if (IsNullConstant(binaryExpression.Right))
    //                {
    //                    _where.Add("IS NOT");
    //                }
    //                else
    //                {
    //                    _where.Add("<>");
    //                }
    //                break;

    //            case ExpressionType.LessThan:
    //                _where.Add("<");
    //                break;

    //            case ExpressionType.LessThanOrEqual:
    //                _where.Add("<=");
    //                break;

    //            case ExpressionType.GreaterThan:
    //                _where.Add(">");
    //                break;

    //            case ExpressionType.GreaterThanOrEqual:
    //                _where.Add(">=");
    //                break;

    //            default:
    //                throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", binaryExpression.NodeType));
    //        }

    //        Visit(binaryExpression.Right);
    //        _where.Add(")");
    //        return binaryExpression;
    //    }

    //    protected override Expression VisitConstant(ConstantExpression constantExpression)
    //    {
    //        switch (constantExpression.Value)
    //        {
    //            case null when constantExpression.Value == null:
    //                _where.Add("NULL");
    //                break;

    //            default:

    //                if (true)
    //                {
    //                    _where.Add(CreateParameter(constantExpression.Value).ParameterName);
    //                }

    //                break;
    //        }

    //        return constantExpression;
    //    }

    //    protected override Expression VisitMember(MemberExpression memberExpression)
    //    {
    //        Expression VisitMemberLocal(Expression expression)
    //        {
    //            switch (expression.NodeType)
    //            {
    //                case ExpressionType.Parameter:
    //                    _where.Add($"[{memberExpression.Member.Name}]");
    //                    return memberExpression;

    //                case ExpressionType.Constant:
    //                    _where.Add(CreateParameter(GetValue(memberExpression)).ParameterName);

    //                    return memberExpression;

    //                case ExpressionType.MemberAccess:
    //                    _where.Add(CreateParameter(GetValue(memberExpression)).ParameterName);

    //                    return memberExpression;
    //            }

    //            throw new NotSupportedException(string.Format("The member '{0}' is not supported", memberExpression.Member.Name));
    //        }

    //        if (memberExpression.Expression == null)
    //        {
    //            return VisitMemberLocal(memberExpression);
    //        }

    //        return VisitMemberLocal(memberExpression.Expression);
    //    }

    //    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    //    {
    //        switch (methodCallExpression.Method.Name)
    //        {
    //            case nameof(Queryable.Where) when methodCallExpression.Method.DeclaringType == typeof(Queryable):

    //                Visit(methodCallExpression.Arguments[0]);
    //                var lambda = (LambdaExpression)StripQuotes(methodCallExpression.Arguments[1]);
    //                Visit(lambda.Body);

    //                return methodCallExpression;

    //            case nameof(Queryable.Select):
    //                return ParseExpression(methodCallExpression, _select);

    //            case nameof(Queryable.GroupBy):
    //                return ParseExpression(methodCallExpression, _groupBy);

    //            case nameof(Queryable.Take):
    //                return ParseExpression(methodCallExpression, ref _take);

    //            case nameof(Queryable.Skip):
    //                return ParseExpression(methodCallExpression, ref _skip);

    //            case nameof(Queryable.OrderBy):
    //            case nameof(Queryable.ThenBy):
    //                return ParseExpression(methodCallExpression, _orderBy, "ASC");

    //            case nameof(Queryable.OrderByDescending):
    //            case nameof(Queryable.ThenByDescending):
    //                return ParseExpression(methodCallExpression, _orderBy, "DESC");

    //            case nameof(Queryable.Distinct):
    //                IsDistinct = true;
    //                return Visit(methodCallExpression.Arguments[0]);

    //            case nameof(string.StartsWith):
    //                _where.AddRange(ParseExpression(methodCallExpression, methodCallExpression.Object));
    //                _where.Add("LIKE");
    //                _where.Add(CreateParameter(GetValue(methodCallExpression.Arguments[0]).ToString() + "%").ParameterName);
    //                return methodCallExpression.Arguments[0];

    //            case nameof(string.EndsWith):
    //                _where.AddRange(ParseExpression(methodCallExpression, methodCallExpression.Object));
    //                _where.Add("LIKE");
    //                _where.Add(CreateParameter("%" + GetValue(methodCallExpression.Arguments[0]).ToString()).ParameterName);
    //                return methodCallExpression.Arguments[0];

    //            case nameof(string.Contains):
    //                _where.AddRange(ParseExpression(methodCallExpression, methodCallExpression.Object));
    //                _where.Add("LIKE");
    //                _where.Add(CreateParameter("%" + GetValue(methodCallExpression.Arguments[0]).ToString() + "%").ParameterName);
    //                return methodCallExpression.Arguments[0];

    //            //case nameof(Extensions.ToSqlString):
    //            //    return Visit(methodCallExpression.Arguments[0]);

    //            //case nameof(Extensions.Delete):
    //            //case nameof(Extensions.DeleteAsync):
    //            //    IsDelete = true;
    //            //    return Visit(methodCallExpression.Arguments[0]);

    //            //case nameof(Extensions.Update):
    //            //    return ParseExpression(methodCallExpression, _update);

    //            default:
    //                if (methodCallExpression.Object != null)
    //                {
    //                    _where.Add(CreateParameter(GetValue(methodCallExpression)).ParameterName);
    //                    return methodCallExpression;
    //                }
    //                break;
    //        }

    //        throw new NotSupportedException($"The method '{methodCallExpression.Method.Name}' is not supported");
    //    }

    //    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    //    {
    //        switch (unaryExpression.NodeType)
    //        {
    //            case ExpressionType.Not:
    //                _where.Add("NOT");
    //                Visit(unaryExpression.Operand);
    //                break;

    //            case ExpressionType.Convert:
    //                Visit(unaryExpression.Operand);
    //                break;

    //            default:
    //                throw new NotSupportedException($"The unary operator '{unaryExpression.NodeType}' is not supported");
    //        }
    //        return unaryExpression;
    //    }

    //    private static Expression StripQuotes(Expression expression)
    //    {

    //        while (expression.NodeType == ExpressionType.Quote)
    //        {
    //            expression = ((UnaryExpression)expression).Operand;
    //        }
    //        return expression;
    //    }



    //    [SuppressMessage("Style", "IDE0011:Add braces", Justification = "Easier to read")]
    //    private IEnumerable<string> BuildOrderByStatement()
    //    {
    //        if (Skip.HasValue && _orderBy.Count == 0)                       /**/   yield return "ORDER BY (SELECT NULL)";
    //        else if (_orderBy.Count == 0)                                   /**/   yield break;
    //        else if (_groupBy.Count > 0 && _orderBy[0].StartsWith("[Key]")) /**/   yield return "ORDER BY " + string.Join(", ", _groupBy);
    //        else                                                            /**/   yield return "ORDER BY " + string.Join(", ",_orderBy);

    //        if (Skip.HasValue && Take.HasValue)                             /**/   yield return $"OFFSET {Skip} ROWS FETCH NEXT {Take} ROWS ONLY";
    //        else if (Skip.HasValue && !Take.HasValue)                       /**/   yield return $"OFFSET {Skip} ROWS";
    //    }

    //    [SuppressMessage("Style", "IDE0011:Add braces", Justification = "Easier to read")]
    //    private IEnumerable<string> BuildSelectStatement()
    //    {
    //        yield return "SELECT";

    //        if (IsDistinct)                                 /**/    yield return "DISTINCT";

    //        if (Take.HasValue && !Skip.HasValue)            /**/    yield return $"TOP ({Take.Value})";

    //        if (_select.Count == 0 && _groupBy.Count > 0)   /**/    yield return _groupBy.Select(x => $"MAX({x})").Join(", ");
    //        else if (_select.Count == 0)                    /**/    yield return "*";
    //        else                                            /**/    yield return _select.Join(", ");
    //    }

    //    [SuppressMessage("Style", "IDE0011:Add braces", Justification = "Easier to read")]
    //    private IEnumerable<string> BuildSqlStatement()
    //    {
    //        if (IsDelete)                   /**/   yield return "DELETE";
    //        else if (_update.Count > 0)     /**/   yield return $"UPDATE [{TableName}]";
    //        else                            /**/   yield return Select;

    //        if (_update.Count == 0)         /**/   yield return From;
    //        else if (_update.Count > 0)     /**/   yield return Update;

    //        if (Where != null)              /**/   yield return Where;
    //        if (GroupBy != null)            /**/   yield return GroupBy;
    //        if (OrderBy != null)            /**/   yield return OrderBy;
    //    }

    //    private SqlParameter CreateParameter(object value)
    //    {
    //        string parameterName = $"@p{_parameters.Count}";

    //        var parameter = new SqlParameter()
    //        {
    //            ParameterName = parameterName,
    //            Value = value
    //        };

    //        _parameters.Add(parameter);

    //        return parameter;
    //    }

    //    private object GetEntityType(Expression expression)
    //    {
    //        while (true)
    //        {
    //            switch (expression)
    //            {
    //                case ConstantExpression constantExpression:
    //                    return constantExpression.Value;

    //                case MethodCallExpression methodCallExpression:
    //                    expression = methodCallExpression.Arguments[0];
    //                    continue;

    //                default:
    //                    return null;
    //            }
    //        }
    //    }

    //    private IEnumerable<string> GetNewExpressionString(NewExpression newExpression, string appendString = null)
    //    {
    //        for (int i = 0; i < newExpression.Members.Count; i++)
    //        {
    //            if (newExpression.Arguments[i].NodeType == ExpressionType.MemberAccess)
    //            {
    //                yield return
    //                    appendString == null ?
    //                    $"[{newExpression.Members[i].Name}]" :
    //                    $"[{newExpression.Members[i].Name}] {appendString}";
    //            }
    //            else
    //            {
    //                yield return
    //                    appendString == null ?
    //                    $"[{newExpression.Members[i].Name}] = {CreateParameter(GetValue(newExpression.Arguments[i])).ParameterName}" :
    //                    $"[{newExpression.Members[i].Name}] = {CreateParameter(GetValue(newExpression.Arguments[i])).ParameterName}";
    //            }
    //        }
    //    }

    //    private object GetValue(Expression expression)
    //    {
    //        object GetMemberValue(MemberInfo memberInfo, object container = null)
    //        {
    //            switch (memberInfo)
    //            {
    //                case FieldInfo fieldInfo:
    //                    return fieldInfo.GetValue(container);

    //                case PropertyInfo propertyInfo:
    //                    return propertyInfo.GetValue(container);

    //                default: return null;
    //            }
    //        }

    //        switch (expression)
    //        {
    //            case ConstantExpression constantExpression:
    //                return constantExpression.Value;

    //            case MemberExpression memberExpression when memberExpression.Expression is ConstantExpression constantExpression:
    //                return GetMemberValue(memberExpression.Member, constantExpression.Value);

    //            case MemberExpression memberExpression when memberExpression.Expression is null: // static
    //                return GetMemberValue(memberExpression.Member);

    //            case MethodCallExpression methodCallExpression:
    //                return Expression.Lambda(methodCallExpression).Compile().DynamicInvoke();

    //            case null:
    //                return null;
    //        }

    //        throw new NotSupportedException();
    //    }

    //    private bool IsNullConstant(Expression expression) => expression.NodeType == ExpressionType.Constant && ((ConstantExpression)expression).Value == null;

    //    private IEnumerable<string> ParseExpression(Expression parent, Expression body, string appendString = null)
    //    {
    //        switch (body)
    //        {
    //            case MemberExpression memberExpression:
    //                return appendString == null ?
    //                    new string[] { $"[{memberExpression.Member.Name}]" } :
    //                    new string[] { $"[{memberExpression.Member.Name}] {appendString}" };

    //            case NewExpression newExpression:
    //                return GetNewExpressionString(newExpression, appendString);

    //            case ParameterExpression parameterExpression when parent is LambdaExpression lambdaExpression && lambdaExpression.ReturnType == parameterExpression.Type:
    //                return new string[0];

    //            case ConstantExpression constantExpression:
    //                return constantExpression
    //                    .Type
    //                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
    //                    .Select(x => $"[{x.Name}] = {CreateParameter(x.GetValue(constantExpression.Value)).ParameterName}");
    //        }

    //        throw new NotSupportedException();
    //    }

    //    private Expression ParseExpression(MethodCallExpression expression, List<string> commandList, string appendString = null)
    //    {
    //        var unary = (UnaryExpression)expression.Arguments[1];
    //        var lambdaExpression = (LambdaExpression)unary.Operand;

    //        lambdaExpression = (LambdaExpression)Evaluator.PartialEval(lambdaExpression);

    //        commandList.AddRange(ParseExpression(lambdaExpression, lambdaExpression.Body, appendString));

    //        return Visit(expression.Arguments[0]);
    //    }

    //    private Expression ParseExpression(MethodCallExpression expression, ref int? size)
    //    {
    //        var sizeExpression = (ConstantExpression)expression.Arguments[1];

    //        if (int.TryParse(sizeExpression.Value.ToString(), out int value))
    //        {
    //            size = value;
    //            return Visit(expression.Arguments[0]);
    //        }

    //        throw new NotSupportedException();
    //    }
    //}


   
    public class Class2
    {
        //public void Test()
        //{
        //    var a = new TestRepository();
        //    var b = a.Select(it => it).ToList();
        //    var f = a.Where(it => it.Name == "hzp");
        //    var d = f.Expression;

        //    //.ToList();
        //    //.OrderBy(it=>it.Age).Skip(1).Take(1).ToList();
        //}
    }

   

   
    
}
