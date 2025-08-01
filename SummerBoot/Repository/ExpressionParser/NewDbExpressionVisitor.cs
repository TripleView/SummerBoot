using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using SqlParser.Net;
using SqlParser.Net.Ast.Expression;
using SummerBoot.Core;
using SummerBoot.Repository.Core;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.ExpressionParser.Util;
using YamlDotNet.Core.Tokens;

namespace SummerBoot.Repository.ExpressionParser;

public class NewDbExpressionVisitor : ExpressionVisitor
{
    private DbType dbType;

    public NewDbExpressionVisitor(DatabaseUnit databaseUnit)
    {
        this.databaseUnit = databaseUnit;
        var databaseType = databaseUnit.DatabaseType;
        switch (databaseType)
        {
            case DatabaseType.SqlServer:
                dbType = DbType.SqlServer;
                leftQualifiers = "[";
                rightQualifiers = "]";
                prefix = "@";
                lengthName = "len";
                break;
            case DatabaseType.Mysql:
                dbType = DbType.MySql;
                leftQualifiers = "`";
                rightQualifiers = "`";
                prefix = "@";
                lengthName = "length";
                break;
            case DatabaseType.Oracle:
                dbType = DbType.Oracle;
                leftQualifiers = ":";
                rightQualifiers = ":";
                prefix = ":";
                lengthName = "length";
                break;
            case DatabaseType.Sqlite:
                dbType = DbType.Sqlite;
                leftQualifiers = "`";
                rightQualifiers = "`";
                prefix = ":";
                lengthName = "length";
                break;
            case DatabaseType.Pgsql:
                dbType = DbType.Pgsql;
                leftQualifiers = "\"";
                rightQualifiers = "\"";
                prefix = "@";
                lengthName = "length";
                break;
        }
    }
    private Dictionary<string, ColumnExpression> _lastColumns =
        new Dictionary<string, ColumnExpression>();

    private List<SqlExpression> _lastGroupByExpressions = new List<SqlExpression>();

    private static readonly IDictionary<ExpressionType, string> nodeTypeMappings = new Dictionary<ExpressionType, string>
        {
            {ExpressionType.Add, "+"},
            {ExpressionType.And, "AND"},
            {ExpressionType.AndAlso, "AND"},
            {ExpressionType.Divide, "/"},
            {ExpressionType.Equal, "="},
            {ExpressionType.ExclusiveOr, "^"},
            {ExpressionType.GreaterThan, ">"},
            {ExpressionType.GreaterThanOrEqual, ">="},
            {ExpressionType.LessThan, "<"},
            {ExpressionType.LessThanOrEqual, "<="},
            {ExpressionType.Modulo, "%"},
            {ExpressionType.Multiply, "*"},
            {ExpressionType.Negate, "-"},
            {ExpressionType.Not, "NOT"},
            {ExpressionType.NotEqual, "<>"},
            {ExpressionType.Or, "OR"},
            {ExpressionType.OrElse, "OR"},
            {ExpressionType.Subtract, "-"}
        };

    private static readonly IDictionary<ExpressionType, SqlBinaryOperator> nodeTypeSqlBinaryOperatorsMappings = new Dictionary<ExpressionType, SqlBinaryOperator>
    {
        {ExpressionType.Add, SqlBinaryOperator.Add},
        {ExpressionType.And, SqlBinaryOperator.And},
        {ExpressionType.AndAlso, SqlBinaryOperator.And},
        {ExpressionType.Divide, SqlBinaryOperator.Divide},
        {ExpressionType.Equal, SqlBinaryOperator.EqualTo},
        {ExpressionType.ExclusiveOr, SqlBinaryOperator.BitwiseXor },
        {ExpressionType.GreaterThan, SqlBinaryOperator.GreaterThen},
        {ExpressionType.GreaterThanOrEqual, SqlBinaryOperator.GreaterThenOrEqualTo},
        {ExpressionType.LessThan,SqlBinaryOperator.LessThen},
        {ExpressionType.LessThanOrEqual, SqlBinaryOperator.LessThenOrEqualTo},
        {ExpressionType.Modulo, SqlBinaryOperator.Mod},
        {ExpressionType.Multiply, SqlBinaryOperator.Multiply},
        {ExpressionType.Negate, SqlBinaryOperator.Sub},
        {ExpressionType.Not, SqlBinaryOperator.IsNot},
        {ExpressionType.NotEqual,SqlBinaryOperator.NotEqualTo},
        {ExpressionType.Or, SqlBinaryOperator.Or},
        {ExpressionType.OrElse, SqlBinaryOperator.Or},
        {ExpressionType.Subtract, SqlBinaryOperator.Sub}
    };

    private Stack<string> methodCallStack = new Stack<string>();
    private List<string> lastMethodCalls = new List<string>();
    /// <summary>
    /// 当前处理的方法名称，比如select，where
    /// </summary>
    private string MethodName => methodCallStack.Count > 0 ? methodCallStack.Peek() : "";

    #region 本次新增

    private SqlSelectExpression result = new SqlSelectExpression();
    protected DatabaseUnit databaseUnit;
    private readonly DynamicParameters parameters = new DynamicParameters();
    /// <summary>
    /// Mapping between table names and aliases
    /// 表名与别名的映射
    /// </summary>
    private Dictionary<string, string> tableNameAliasMapping = new Dictionary<string, string>();

    /// <summary>
    /// Qualifiers for identifiers
    /// 标识符的限定符
    /// </summary>
    private string leftQualifiers;
    /// <summary>
    /// Qualifiers for identifiers
    /// 标识符的限定符
    /// </summary>
    private string rightQualifiers;
    /// <summary>
    /// Parameter prefix
    /// 参数前缀，如@,:
    /// </summary>
    private string prefix;
    /// <summary>
    /// Database method name mapped to string length
    /// 字符串length映射的数据库方法名
    /// </summary>
    private string lengthName;
    #endregion
    /// <summary>
    /// 上一次处理的方法名称
    /// </summary>
    private string LastMethodName => lastMethodCalls.LastOrDefault();


    #region 表名参数名生成管理

    private int tableIndex = -1;
    private int parameterIndex = -1;
    /// <summary>
    /// Get the expression tree parsing result
    /// 获取表达式树解析结果
    /// </summary>
    /// <returns></returns>
    public ExpressionTreeParsingResult GetParsingResult()
    {
        //If no field is selected, the default value is *
        //在没有选择字段的情况下，默认*
        if (result.Query is SqlSelectQueryExpression sqlSelectQueryExpression
            && LastMethodName != nameof(Queryable.Select)
            && sqlSelectQueryExpression.Columns.Count == 0)
        {
            sqlSelectQueryExpression.Columns.Add(new SqlSelectItemExpression()
            {
                Body = new SqlIdentifierExpression()
                {
                    Value = "*"
                }
            });
        }
        return new ExpressionTreeParsingResult()
        {
            Sql = result.ToSql(dbType).Trim(),
            Parameters = parameters
        };
    }
    /// <summary>
    /// Get table alias
    /// 获取表别名
    /// </summary>
    public string GetTableAlias()
    {
        tableIndex++;
        return "p" + tableIndex;
    }

    /// <summary>
    /// Get parameter alias
    /// 获取参数别名
    /// </summary>
    /// <returns></returns>
    public string GetParameterAlias()
    {
        parameterIndex++;
        return "y" + parameterIndex;
    }
    #endregion

    public override Expression Visit(Expression exp)
    {
        if (exp == null) return null;

        switch ((DbExpressionType)exp.NodeType)
        {
            case DbExpressionType.Select:
            case DbExpressionType.Join:
            case DbExpressionType.Query:
                return this.VisitQuery((QueryExpression)exp);

            case DbExpressionType.MultiSelect:
                return this.VisitMultiSelect((MultiSelectExpression)exp);
            case DbExpressionType.MultiSelectAutoFill:
                return this.VisitMultiSelectAutoFill((MultiSelectAutoFillExpression)exp);
            case DbExpressionType.MultiQueryWhere:
                return this.VisitMultiQueryWhere((MultiQueryWhereAdapterExpression)exp);
            case DbExpressionType.MultiQueryOrderBy:
                return this.VisitMultiQueryOrderBy((MultiQueryOrderByAdapterExpression)exp);

        }

        return base.Visit(exp);
    }
    public virtual Expression VisitMultiQueryOrderBy(MultiQueryOrderByAdapterExpression orderByAdapterExpression)
    {
        methodCallStack.Push(nameof(RepositoryMethod.MultiQueryOrderBy));
        var result = Visit(orderByAdapterExpression.OnExpression);
        methodCallStack.Pop();
        return result;


    }

    public virtual Expression VisitMultiSelect(MultiSelectExpression multiSelectExpression)
    {
        if (multiSelectExpression.Expression is NewExpression newExpression)
        {
            var result = VisitMultiSelectNew(newExpression);
            return result;
        }
        else if (multiSelectExpression.Expression is MemberInitExpression memberInitExpression)
        {
            var result = VisitMultiSelectMemberInit(memberInitExpression);
            return result;
        }
        else
        {
            methodCallStack.Push(nameof(RepositoryMethod.MultiSelect));
            var result = Visit(multiSelectExpression.Expression);
            methodCallStack.Pop();
            return result;
        }

        throw new NotSupportedException(nameof(multiSelectExpression.Expression));
    }

    public virtual Expression VisitMultiSelectAutoFill(MultiSelectAutoFillExpression multiSelectAutoFillExpression)
    {
        if (multiSelectAutoFillExpression.Expression is MemberExpression memberExpression)
        {
            var tableAlias = memberExpression.Member.Name;
            var table = new TableExpression(multiSelectAutoFillExpression.Expression.Type, tableAlias);
            return new ColumnsExpression(table.Columns, memberExpression.Type);
        }

        throw new NotSupportedException(nameof(multiSelectAutoFillExpression));
    }

    public virtual Expression VisitMultiQueryWhere(MultiQueryWhereAdapterExpression multiQueryWhereAdapterExpression)
    {
        methodCallStack.Push(nameof(RepositoryMethod.MultiQueryWhere));
        var result = Visit(multiQueryWhereAdapterExpression.OnExpression);
        methodCallStack.Pop();
        return result;

    }

    public virtual Expression VisitQuery(QueryExpression queryExpression)
    {
        return queryExpression;
    }

    public virtual Expression VisitSelect(SelectExpression selectExpression)
    {
        return selectExpression;
    }

    private string BoxMethodLikeParameter(object parameter, string methodName)
    {
        switch (methodName)
        {
            case "Contains":
                return $"%{parameter}%";
                break;
            case "StartsWith":
                return $"{parameter}%";
                break;
            case "EndsWith":
                return $"%{parameter}";
                break;
        }

        throw new NotSupportedException(methodName);
    }
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var method = node.Method;
        var methodName = node.Method.Name;
        switch (method.Name)
        {
            case nameof(Queryable.Select):
                methodCallStack.Push(methodName);
                var result = this.VisitSelectCall(node);
                var lastMethodCallName = methodCallStack.Pop();
                lastMethodCalls.Add(lastMethodCallName);
                return result;
            //get_Item方法的意思是从list里取数，比如list[0]
            case "get_Item":
                if (node.Arguments.Count == 1 && node.Arguments[0] is ConstantExpression constantExpression)
                {
                    var obj = GetValue(node.Object);
                    var value = method.Invoke(obj, new[] { constantExpression.Value });
                    return Expression.Constant(value);
                }
                else
                {
                    throw new NotSupportedException("get_Item");
                }
                break;
            case nameof(Queryable.Where):
            case nameof(QueryableMethodsExtension.OrWhere):
                methodCallStack.Push(method.Name);
                //MethodName = method.Name;
                var result2 = this.VisitWhereCall(node);
                var lastMethodCallName2 = methodCallStack.Pop();
                lastMethodCalls.Add(lastMethodCallName2);
                return result2;
            case nameof(Queryable.GroupBy):
                methodCallStack.Push(methodName);
                //MethodName = method.Name;
                var result3 = this.VisitGroupByCall(node);
                var lastMethodCallName3 = methodCallStack.Pop();
                lastMethodCalls.Add(lastMethodCallName3);
                return result3;
            case nameof(Queryable.OrderBy):
            case nameof(Queryable.OrderByDescending):
            case nameof(Queryable.ThenBy):
            case nameof(Queryable.ThenByDescending):
                //MethodName = method.Name;
                methodCallStack.Push(methodName);
                var result4 = this.VisitOrderByCall(node);
                var lastMethodCallName4 = methodCallStack.Pop();
                lastMethodCalls.Add(lastMethodCallName4);
                return result4;
            case nameof(Queryable.First):
            case nameof(Queryable.FirstOrDefault):
                methodCallStack.Push(methodName);
                var result6 = this.VisitFirstOrDefault(node);
                var lastMethodCallName6 = methodCallStack.Pop();
                lastMethodCalls.Add(lastMethodCallName6);
                break;
            case nameof(Queryable.Distinct):
            case nameof(Queryable.Count):
                //针对group by里的count单独处理
                if (methodName == nameof(Queryable.Count) && LastMethodName == nameof(Queryable.GroupBy))
                {
                    break;
                }
                methodCallStack.Push(methodName);
                var result5 = this.VisitFirstOrDefaultDistinctCall(node);
                var lastMethodCallName5 = methodCallStack.Pop();
                lastMethodCalls.Add(lastMethodCallName5);
                return result5;
            case nameof(Queryable.Skip):
            case nameof(Queryable.Take):
                methodCallStack.Push(methodName);

                var result7 = this.VisitSkipTakeCall(node);
                var lastMethodCallName7 = methodCallStack.Pop();
                lastMethodCalls.Add(lastMethodCallName7);
                return result7;
            case nameof(Queryable.Max):
            case nameof(Queryable.Min):
            case nameof(Queryable.Sum):
            case nameof(Queryable.Average):
                //针对group by里的count单独处理
                if (LastMethodName == nameof(Queryable.GroupBy))
                {
                    break;
                }
                methodCallStack.Push(methodName);
                var result8 = this.VisitMaxMinSumAvgCall(node);
                var lastMethodCallName8 = methodCallStack.Pop();
                lastMethodCalls.Add(lastMethodCallName8);
                return result8;

        }

        //针对groupBy进行单独处理
        if (LastMethodName == nameof(Queryable.GroupBy))
        {
            SqlExpression body = null;
            switch (methodName)
            {
                case nameof(Queryable.Count):
                    body = _lastGroupByExpressions.Last();
                    break;
                case nameof(Queryable.Max):
                case nameof(Queryable.Min):
                case nameof(Queryable.Sum):
                case nameof(Queryable.Average):
                    if (methodName == nameof(Queryable.Average))
                    {
                        methodName = "Avg";
                    }

                    var lambda = (LambdaExpression)this.StripQuotes(node.Arguments[1]);
                    var value = this.Visit(lambda.Body);
                    body = GetSqlExpression(value);
                    break;

            }

            var sqlFunctionCallExpression = new SqlFunctionCallExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = methodName
                },
                Arguments = new List<SqlExpression>()
                {
                    body
                }
            };
            return GetWrapperExpression(sqlFunctionCallExpression);
        }



        var containsMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
        var startsWithMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
        var endsWithMethod = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });

        var stringMethodLikeInfoList = new List<MethodInfo>
            {
                containsMethod,
                startsWithMethod,
                endsWithMethod
            };

        var trimMethod = typeof(string).GetMethod("Trim", new Type[] { });
        var trimLeftMethod = typeof(string).GetMethod("TrimStart", new Type[] { });
        var trimRightMethod = typeof(string).GetMethod("TrimEnd", new Type[] { });
        var toUpperMethod = typeof(string).GetMethod("ToUpper", new Type[] { });
        var toLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });

        var stringMethodTrimUpperLowerInfoList = new List<MethodInfo>
            {
                trimMethod,
                trimLeftMethod,
                trimRightMethod,
                toUpperMethod,
                toLowerMethod
            };

        if (method.DeclaringType == null)
        {
            throw new NotSupportedException(methodName);
        }

        //处理string
        if (method.DeclaringType == typeof(string))
        {
            if (stringMethodLikeInfoList.Contains(node.Method))
            {
                var objectExpression = this.Visit(node.Object);
                var likeLeft = "";
                var likeRight = "";
                if (node.Method == containsMethod)
                {
                    likeLeft = "%";
                    likeRight = "%";
                }
                else if (node.Method == startsWithMethod)
                {
                    likeLeft = "";
                    likeRight = "%";
                }
                else if (node.Method == endsWithMethod)
                {
                    likeLeft = "%";
                    likeRight = "";
                }
                var leftSqlExpression = GetSqlExpression(objectExpression);
                if (node.Arguments.Count > 0)
                {
                    Expression sqlConstExpression = null;
                    //it.Name.Contains("hzp")
                    if (node.Arguments[0] is ConstantExpression constantExpression)
                    {
                        sqlConstExpression = GetConstExpression(constantExpression);
                    }
                    //it.Name.Contains(pet.Name)
                    else if (node.Arguments[0] is MemberExpression memberExpression)
                    {
                        sqlConstExpression = this.Visit(memberExpression);
                    }
                    else
                    {
                        sqlConstExpression = this.Visit(node.Arguments[0]);
                    }
                    AddLikeWildcards(sqlConstExpression, likeLeft, likeRight);
                    var rightSqlExpression = GetSqlExpression(sqlConstExpression);
                    var binaryExpression = new SqlBinaryExpression()
                    {
                        Left = leftSqlExpression,
                        Operator = SqlBinaryOperator.Like,
                        Right = rightSqlExpression
                    };

                    return GetWrapperExpression(binaryExpression);
                }
                else
                {
                    throw new NotSupportedException(methodName);
                }

            }
            else if (stringMethodTrimUpperLowerInfoList.Contains(node.Method))
            {
                var objectExpression = this.Visit(node.Object);

                var sqlExpression = GetSqlExpression(objectExpression);
                var tempMethodName = "";
                if (node.Method == trimMethod)
                {
                    tempMethodName = "trim";
                }
                else if (node.Method == trimLeftMethod)
                {
                    tempMethodName = "ltrim";
                }
                else if (node.Method == trimRightMethod)
                {
                    tempMethodName = "rtrim";
                }
                else if (node.Method == toUpperMethod)
                {
                    tempMethodName = "upper";
                }
                else if (node.Method == toLowerMethod)
                {
                    tempMethodName = "lower";
                }
                var result = new SqlFunctionCallExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = tempMethodName
                    },
                    Arguments = new List<SqlExpression>()
                    {
                        sqlExpression
                    }
                };
                
                return GetWrapperExpression(result);
                if (objectExpression is ColumnExpression columnExpression)
                {
                    if (node.Method == trimMethod)
                    {
                        columnExpression.FunctionName = "TRIM";
                    }
                    else if (node.Method == trimLeftMethod)
                    {
                        columnExpression.FunctionName = "LTRIM";
                    }
                    else if (node.Method == trimRightMethod)
                    {
                        columnExpression.FunctionName = "RTRIM";
                    }
                    else if (node.Method == toUpperMethod)
                    {
                        columnExpression.FunctionName = "UPPER";
                    }
                    else if (node.Method == toLowerMethod)
                    {
                        columnExpression.FunctionName = "LOWER";
                    }
                    return columnExpression;

                }

                else if (objectExpression is ConstantExpression constantExpression && (constantExpression.Type == typeof(string) || constantExpression.Value is null))
                {
                    if (constantExpression.Value is null)
                    {
                        return constantExpression;
                    }
                    if (node.Method == trimMethod)
                    {
                        return Expression.Constant(constantExpression.Value.ToString().Trim());
                    }
                    else if (node.Method == trimLeftMethod)
                    {
                        return Expression.Constant(constantExpression.Value.ToString().TrimStart());
                    }
                    else if (node.Method == trimRightMethod)
                    {
                        return Expression.Constant(constantExpression.Value.ToString().TrimEnd());
                    }
                    else if (node.Method == toUpperMethod)
                    {
                        return Expression.Constant(constantExpression.Value.ToString().ToUpper());
                    }
                    else if (node.Method == toLowerMethod)
                    {
                        return Expression.Constant(constantExpression.Value.ToString().ToLower());
                    }
                    return constantExpression;
                }
                else
                {
                    throw new NotSupportedException($"not support method name:{methodName}");
                }

            }
            else if (methodName == "Equals" && node.Arguments.Count == 1)
            {
                var objectExpression = this.Visit(node.Object);
                var left = GetSqlExpression(objectExpression);
                var constExpression = this.Visit(node.Arguments[0]);
                var right = GetSqlExpression(constExpression);
                var binaryExpression = new SqlBinaryExpression()
                {
                    Left = left,
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = right
                };
              
                return GetWrapperExpression(binaryExpression);
                if (objectExpression is ColumnExpression columnExpression)
                {
                    if (node.Arguments[0] is ConstantExpression constantExpression)
                    {
                        var whereConditionExpression = new WhereConditionExpression(columnExpression, "=", constantExpression.Value);
                        return whereConditionExpression;
                    }
                    else if (node.Arguments[0] is MemberExpression memberExpression)
                    {
                        var memberResultExpression = this.Visit(memberExpression);
                        if (memberResultExpression is ConstantExpression constantExpression2)
                        {
                            var whereConditionExpression2 = new WhereConditionExpression(columnExpression, "=", constantExpression2.Value);
                            return whereConditionExpression2;
                        }
                        else
                        {
                            throw new NotSupportedException(methodName);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException(methodName);
                    }

                }
                else
                {
                    throw new NotSupportedException(methodName);
                }
            }
            else
            {
                throw new NotSupportedException(methodName);
            }
        }
        else if (method.DeclaringType == typeof(DateTime))
        {
            var date = this.GetValue(node);
            return ConstantExpression.Constant(date);
        }
        else
        {
            //collection
            var isIEnumerable = method.DeclaringType.GetInterfaces().Contains(typeof(IEnumerable)) &&
                                node.Arguments.Count == 1;
            var isArray = method.DeclaringType == typeof(Enumerable) &&
                          method.GetCustomAttribute<ExtensionAttribute>() != null && node.Arguments.Count == 2;

            if (isIEnumerable || isArray)
            {
                Expression collection;
                Expression property;
                if (isIEnumerable)
                {
                    collection = node.Object;
                    property = node.Arguments[0];
                }
                else
                {
                    collection = node.Arguments[0];
                    property = node.Arguments[1];

                }

                var values = (IEnumerable)GetValue(collection);
                var propertyExpression = this.Visit(property);
                var body = GetSqlExpression(propertyExpression);
                var count = 0;
                foreach (var value in values)
                {
                    count++;
                }

                var i = 1;
                var sqlInExpressions = new List<SqlInExpression>();
                var parameterNames = new List<string>();
                foreach (var value in values)
                {
                    var parameterName = GetParameterAlias();
                    parameterNames.Add(parameterName);
                    this.parameters.Add(parameterName, value);
                    if ((i != 1 && i % 500 == 0) || i == count)
                    {
                        var targetList = new List<SqlExpression>();
                        foreach (var name in parameterNames)
                        {
                            targetList.Add(new SqlVariableExpression()
                            {
                                Prefix = prefix,
                                Name = name
                            });
                        }
                        var result = new SqlInExpression()
                        {
                            Body = body,
                            TargetList = targetList
                        };
                        sqlInExpressions.Add(result);

                        parameterNames.Clear();
                    }

                    i++;
                }

                if (sqlInExpressions.Count > 1)
                {
                    SqlExpression first = sqlInExpressions.First();
                    sqlInExpressions.RemoveAt(0);
                    foreach (var sqlInExpression in sqlInExpressions)
                    {
                        first = new SqlBinaryExpression()
                        {
                            Left = first,
                            Right = sqlInExpression,
                            Operator = SqlBinaryOperator.Or
                        };
                    }

                    return GetWrapperExpression(first);
                }
                else
                {
                    return GetWrapperExpression(sqlInExpressions.First());
                }

            }
            else
            {
                var date = this.GetValue(node);
                return ConstantExpression.Constant(date);
            }
        }

        return node;
    }

    protected WhereExpression FoldWhereExpression(List<WhereConditionExpression> whereConditionExpressions)
    {
        WhereExpression result = null;
        for (var i = 0; i < whereConditionExpressions.Count; i++)
        {
            var whereConditionExpression = whereConditionExpressions[i];
            if (i == 0)
            {
                result = whereConditionExpression;
            }
            else
            {
                WhereExpression tempResult = new WhereExpression("or")
                {
                    Left = result,
                    Right = whereConditionExpression
                };
                result = tempResult;
            }
        }

        return result;
    }

    /// <summary>
    /// 去除表达式中的参数引用包装
    /// </summary>
    public Expression StripQuotes(Expression e)
    {
        //如果为参数应用表达式
        while (e.NodeType == ExpressionType.Quote)
        {
            //将其转为一元表达式即可获取真正的值
            e = ((UnaryExpression)e).Operand;
        }
        return e;
    }

    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        if (MethodName == nameof(Queryable.Select))
        {

        }
        else if (MethodName == nameof(QueryableMethodsExtension.OrWhere) || MethodName == nameof(Queryable.Where) || MethodName == nameof(Queryable.FirstOrDefault) || MethodName == nameof(Queryable.First) || MethodName == nameof(Queryable.Count))
        {
            if (!nodeTypeSqlBinaryOperatorsMappings.TryGetValue(binaryExpression.NodeType, out var sqlBinaryOperator))
            {
                throw new NotSupportedException(nameof(binaryExpression.NodeType));
            }

            var rightExpression = this.Visit(binaryExpression.Right);
            var leftExpression = this.Visit(binaryExpression.Left);
            AdaptingBooleanProperties(leftExpression, rightExpression);
            var left = GetSqlExpression(leftExpression);
            var right = GetSqlExpression(rightExpression);

            var result = new SqlBinaryExpression()
            {
                Left = left,
                Right = right,
                Operator = sqlBinaryOperator
            };

            return GetWrapperExpression(result);
        }
        else if (MethodName == nameof(RepositoryMethod.MultiQueryWhere))
        {
            var @operator = nodeTypeMappings[binaryExpression.NodeType];
            var result = new WhereExpression(@operator);
            if (string.IsNullOrWhiteSpace(@operator))
            {
                throw new NotSupportedException(nameof(binaryExpression.NodeType));
            }

            Expression rightExpression;
            if (binaryExpression.Right is UnaryExpression rightEx)
            {
                rightExpression = this.Visit(rightEx.Operand);
            }
            else
            {
                rightExpression = this.Visit(binaryExpression.Right);
            }

            Expression leftExpression;
            if (binaryExpression.Left is UnaryExpression leftEx)
            {
                leftExpression = this.Visit(leftEx.Operand);
            }
            else
            {
                leftExpression = this.Visit(binaryExpression.Left);
            }

            if (leftExpression is ColumnExpression leftColumnExpression && rightExpression is ConstantExpression rightConstantExpression)
            {
                return new WhereConditionExpression(leftColumnExpression, @operator,
                    rightConstantExpression.Value)
                {
                    ValueType = leftColumnExpression.ValueType
                };
            }
            else if (rightExpression is ColumnExpression rightColumnExpression && leftExpression is ConstantExpression leftConstantExpression)
            {
                return new WhereConditionExpression(rightColumnExpression, @operator,
                    leftConstantExpression.Value)
                {
                    ValueType = rightColumnExpression.ValueType
                };
            }

            else if (leftExpression is ColumnExpression leftColumnExpression2 && leftColumnExpression2.Type == typeof(bool) && rightExpression is WhereExpression rightWhereExpression2)
            {
                //如果是column类型的bool值，默认为true
                var left = new WhereConditionExpression(leftColumnExpression2, "=", 1)
                {
                    ValueType = leftColumnExpression2.ValueType
                };
                result.Left = left;
                result.Right = rightWhereExpression2;
                return result;
            }
            else if (rightExpression is ColumnExpression rightColumnExpression2 && rightExpression.Type == typeof(bool) && leftExpression is WhereExpression leftWhereExpression2)
            {
                //如果是column类型的bool值，默认为true
                var right = new WhereConditionExpression(rightColumnExpression2, "=", 1) { ValueType = rightColumnExpression2.ValueType };
                result.Left = leftWhereExpression2;
                result.Right = right;
                return result;
            }
            else if (rightExpression is ColumnExpression rightColumnExpression3 && rightExpression.Type == typeof(bool) && leftExpression is ColumnExpression leftColumnExpression3 && leftColumnExpression3.Type == typeof(bool))
            {
                //如果是column类型的bool值，默认为true
                var right = new WhereConditionExpression(rightColumnExpression3, "=", 1) { ValueType = rightColumnExpression3.ValueType };
                var left = new WhereConditionExpression(leftColumnExpression3, "=", 1) { ValueType = leftColumnExpression3.ValueType };
                result.Left = left;
                result.Right = right;
                return result;
            }
            else if (leftExpression is WhereExpression leftWhereExpression &&
               rightExpression is WhereExpression rightWhereExpression)
            {
                result.Left = leftWhereExpression;
                result.Right = rightWhereExpression;
                return result;

            }
            //兼容It=>true这种
            else if (leftExpression is ConstantExpression constantExpression && constantExpression.Type == typeof(bool) && rightExpression is WhereExpression whereExpression)
            {
                var value = (bool)constantExpression.Value;
                var whereTrueFalseValueCondition = new WhereTrueFalseValueConditionExpression(value)
                {
                    ValueType = typeof(bool)
                };
                result.Left = whereTrueFalseValueCondition;
                result.Right = whereExpression;
                return result;
            }
            //兼容It=>true这种
            else if (rightExpression is ConstantExpression rightConstantExpression3 && rightConstantExpression3.Type == typeof(bool) && leftExpression is WhereExpression leftWhereExpression3)
            {
                var value = (bool)rightConstantExpression3.Value;
                var whereTrueFalseValueCondition = new WhereTrueFalseValueConditionExpression(value)
                {
                    ValueType = typeof(bool)
                };
                result.Left = leftWhereExpression3;
                result.Right = whereTrueFalseValueCondition;
                return result;
            }
            else if (rightExpression is ColumnExpression rightColumnExpression4 && leftExpression is ColumnExpression leftColumnExpression4)
            {

                var whereTrueFalseValueCondition =
                    new WhereTwoColumnExpression(leftColumnExpression4, @operator, rightColumnExpression4);

                return whereTrueFalseValueCondition;
            }
            else
            {
                throw new NotSupportedException(MethodName);
            }


        }
        //throw new NotSupportedException(MethodName);
        return base.VisitBinary(binaryExpression);
    }

    /// <summary>
    /// 适配布尔属性
    /// Adapting Boolean properties
    /// </summary>
    /// <param name="leftExpression"></param>
    /// <param name="rightExpression"></param>
    /// <exception cref="NotSupportedException"></exception>
    private void AdaptingBooleanProperties(Expression leftExpression, Expression rightExpression)
    {
        if (leftExpression is WrapperExpression left && rightExpression is WrapperExpression right)
        {
            //it.HaveChildren && it.HaveChildren 
            if (right.PropertyType == typeof(bool) && left.PropertyType == typeof(bool))
            {
                right.SqlExpression = new SqlBinaryExpression()
                {
                    Left = right.SqlExpression,
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlNumberExpression()
                    {
                        Value = 1m
                    }
                };
                left.SqlExpression = new SqlBinaryExpression()
                {
                    Left = left.SqlExpression,
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlNumberExpression()
                    {
                        Value = 1m
                    }
                };
            }
            //it.HaveChildren && it.Name == "hzp"
            else if (left.PropertyType == typeof(bool) && right.SqlExpression is not SqlBoolExpression)
            {
                left.SqlExpression = new SqlBinaryExpression()
                {
                    Left = left.SqlExpression,
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlNumberExpression()
                    {
                        Value = 1m
                    }
                };
            }
            //it.Name == "hzp" && it.HaveChildren 
            else if (right.PropertyType == typeof(bool) && left.SqlExpression is not SqlBoolExpression)
            {
                right.SqlExpression = new SqlBinaryExpression()
                {
                    Left = right.SqlExpression,
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlNumberExpression()
                    {
                        Value = 1m
                    }
                };
            }
            //false ==  it.HaveChildren 
            else if (right.PropertyType == typeof(bool) && left.SqlExpression is SqlBoolExpression sqlBoolExpression)
            {
                left.SqlExpression = new SqlNumberExpression()
                {
                    Value = sqlBoolExpression.Value ? 1 : 0
                };
            }
            //it.HaveChildren == false 
            else if (left.PropertyType == typeof(bool) && right.SqlExpression is SqlBoolExpression sqlBoolExpression2)
            {
                right.SqlExpression = new SqlNumberExpression()
                {
                    Value = sqlBoolExpression2.Value ? 1 : 0
                };
            }
            //it.Name == "hzp" && true
            else if (left.PropertyType == null && right.SqlExpression is SqlBoolExpression sqlBoolExpression3)
            {
                ConvertBooleanValueToSqlBinaryExpression(right);
            }
            //true && it.Name == "hzp"
            else if (right.PropertyType == null && left.SqlExpression is SqlBoolExpression sqlBoolExpression4)
            {
                ConvertBooleanValueToSqlBinaryExpression(left);
            }
        }

    }

    /// <summary>
    /// Convert Boolean values to sql binary expressions
    /// 把布尔值转为sql二元表达式
    /// </summary>
    /// <param name="wrapperExpression"></param>
    private void ConvertBooleanValueToSqlBinaryExpression(WrapperExpression wrapperExpression)
    {
        if (wrapperExpression.SqlExpression is SqlBoolExpression sqlBoolExpression4)
        {
            wrapperExpression.SqlExpression = new SqlBinaryExpression()
            {
                Left = new SqlNumberExpression()
                {
                    Value = 1
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlNumberExpression()
                {
                    Value = sqlBoolExpression4.Value ? 1 : 0
                },
            };
        }
    }

    private SqlExpression GetSqlExpression(Expression expression)
    {
        if (expression is WrapperExpression wrapperExpression)
        {
            return wrapperExpression.SqlExpression;
        }

        throw new NotSupportedException(nameof(expression));
    }


    public virtual Expression VisitFirstOrDefault(MethodCallExpression firstOrDefaultCall)
    {
        var methodName = firstOrDefaultCall.Method.Name;
        var sourceExpression = this.Visit(firstOrDefaultCall.Arguments[0]);
        var sqlExpression = GetSqlExpression(sourceExpression);
        if (sqlExpression is SqlSelectExpression {Query:SqlSelectQueryExpression sqlSelectQueryExpression})
        {
            sqlSelectQueryExpression.Limit = new SqlLimitExpression()
            {
                Offset = new SqlNumberExpression()
                {
                    Value = -1
                },
                RowCount = new SqlNumberExpression()
                {
                    Value = 1
                },
            };
        }

        return GetWrapperExpression(sqlExpression);
    }


    public virtual Expression VisitFirstOrDefaultDistinctCall(MethodCallExpression firstOrDefaultCall)
    {
        var methodName = firstOrDefaultCall.Method.Name;
        var sourceExpression = this.Visit(firstOrDefaultCall.Arguments[0]);
        WhereExpression where = null;
        if (firstOrDefaultCall.Arguments.Count == 2)
        {
            var lambda = (LambdaExpression)this.StripQuotes(firstOrDefaultCall.Arguments[1]);
            where = this.Visit(lambda.Body) as WhereExpression;
        }



        if (sourceExpression is TableExpression table)
        {
            var result = new SelectExpression(null, "", table.Columns, table);
            if (methodName == nameof(Queryable.FirstOrDefault) || methodName == nameof(Queryable.First))
            {
                result.Take = 1;
            }
            else if (methodName == nameof(Queryable.Distinct))
            {
                result.ColumnsPrefix = "DISTINCT";
            }
            else if (methodName == nameof(Queryable.Count))
            {
                result.Columns.Clear();
                result.Columns.Add(new ColumnExpression(null, "", null, 0, "", "Count"));
            }
            else
            {
                throw new NotSupportedException(nameof(firstOrDefaultCall));
            }

            if (where != null)
            {
                result.Where = where;
            }

            return result;
        }
        else if (sourceExpression is SelectExpression selectExpression)
        {
            if (!selectExpression.HasGroupBy)
            {
                selectExpression = NestSelectExpression(selectExpression);
            }


            if (methodName == nameof(Queryable.FirstOrDefault) || methodName == nameof(Queryable.First))
            {
                selectExpression.Take = 1;
            }
            else if (methodName == nameof(Queryable.Distinct))
            {
                selectExpression.ColumnsPrefix = "DISTINCT";
            }
            else
            {
                throw new NotSupportedException(nameof(firstOrDefaultCall));
            }

            if (where != null)
            {
                selectExpression.Where = where;
            }

            return selectExpression;
        }
        else
        {
            throw new NotSupportedException(nameof(firstOrDefaultCall));
        }
    }

    public virtual Expression VisitMaxMinSumAvgCall(MethodCallExpression maxMinCall)
    {
        var methodName = maxMinCall.Method.Name;

        var sourceExpression = this.Visit(maxMinCall.Arguments[0]);
        ColumnExpression column = null;
        if (maxMinCall.Arguments.Count == 2)
        {
            var lambda = (LambdaExpression)this.StripQuotes(maxMinCall.Arguments[1]);
            column = this.Visit(lambda.Body) as ColumnExpression;
        }
        else
        {
            throw new NotSupportedException(methodName);
        }

        column.FunctionName = methodName;
        if (methodName == nameof(Queryable.Average))
        {
            column.FunctionName = "AVG";
        }

        if (sourceExpression is TableExpression table)
        {
            var result = new SelectExpression(null, "", table.Columns, table);

            result.Columns.Clear();

            result.Columns.Add(column);

            return result;
        }
        else if (sourceExpression is SelectExpression selectExpression)
        {
            selectExpression.Columns.Clear();
            selectExpression.Columns.Add(column);
            return selectExpression;
        }
        else
        {
            throw new NotSupportedException(methodName);
        }
    }


    protected SelectExpression NestSelectExpression(SelectExpression selectExpression)
    {
        //如果是distinct，需要终结小查询，并作为子查询提供给下一个环节，比例原始数据为1,1,2，
        //如果先分页，再distinct，结果可能是11，但如果先distinct再分页，结果可能是1,2
        if (selectExpression.ColumnsPrefix == "DISTINCT")
        {
            var newSelectExpression = new SelectExpression(null, selectExpression.Alias, selectExpression.Columns,
                selectExpression);
            selectExpression = newSelectExpression;
        }

        return selectExpression;
    }

    public virtual Expression VisitSkipTakeCall(MethodCallExpression skipTakExpression)
    {
        var methodName = skipTakExpression.Method.Name;
        var sourceExpression = this.Visit(skipTakExpression.Arguments[0]);
        var countExpression = this.Visit(skipTakExpression.Arguments[1]);
        if (!(countExpression is ConstantExpression constantExpression))
        {
            throw new ArgumentNullException("count");
        }

        var count = (int)constantExpression.Value;

        if (sourceExpression is TableExpression table)
        {
            var result = new SelectExpression(null, "", table.Columns, table);
            if (methodName == nameof(Queryable.Skip))
            {
                result.Skip = count;
            }
            else if (methodName == nameof(Queryable.Take))
            {
                result.Take = count;
            }
            else
            {
                throw new NotSupportedException(nameof(skipTakExpression));
            }

            return result;
        }
        else if (sourceExpression is SelectExpression selectExpression)
        {
            if (!selectExpression.HasGroupBy)
            {
                selectExpression = NestSelectExpression(selectExpression);
            }

            if (methodName == nameof(Queryable.Skip))
            {
                selectExpression.Skip = count;
            }
            else if (methodName == nameof(Queryable.Take))
            {
                selectExpression.Take = count;
            }
            else
            {
                throw new NotSupportedException(nameof(skipTakExpression));
            }
            return selectExpression;
        }
        else
        {
            throw new NotSupportedException(nameof(skipTakExpression));
        }
    }

    public virtual Expression VisitWhereCall(MethodCallExpression whereCall)
    {
        if (MethodName == nameof(Queryable.GroupBy))
        {
            return whereCall;
        }
        else
        {
            var @operator = MethodName == nameof(QueryableMethodsExtension.OrWhere) ? "or" : "and";
            this.Visit(whereCall.Arguments[0]);
            var lambda = (LambdaExpression)this.StripQuotes(whereCall.Arguments[1]);
            var bodyExpression = this.Visit(lambda.Body);
            if (bodyExpression is WrapperExpression tempBodyExpression)
            {
                ConvertBooleanValueToSqlBinaryExpression(tempBodyExpression);
            }
            
            var where = GetSqlExpression(bodyExpression);
            
            if (result.Query is SqlSelectQueryExpression sqlSelectQueryExpression)
            {
                sqlSelectQueryExpression.Where = where;
            }
           
            return null;
        }

    }

    private SelectExpression CombineSelectExpression(TableExpression source, WhereExpression whereExpression, SelectExpression selectExpression, string @operator)
    {
        if (selectExpression == null)
        {
            var result = new SelectExpression(null, "", source.Columns, source, whereExpression);
            return result;
        }

        if (selectExpression.Where != null)
        {
            var tempWhereExpression = new WhereExpression(@operator)
            {
                Left = selectExpression.Where,
                Right = whereExpression
            };
            selectExpression.Where = tempWhereExpression;
        }
        else
        {
            selectExpression.Where = whereExpression;
        }
        return selectExpression;
    }

    public virtual Expression VisitOrderByCall(MethodCallExpression whereCall)
    {
        var sourceExpression = this.Visit(whereCall.Arguments[0]);
        var lambda = (LambdaExpression)this.StripQuotes(whereCall.Arguments[1]);
        var bodyExpression = this.Visit(lambda.Body);
        var sourceSqlExpression = GetSqlExpression(sourceExpression);
        var bodySqlExpression = GetSqlExpression(bodyExpression);

        SqlOrderByType orderByType;
        switch (whereCall.Method.Name)
        {
            case nameof(Queryable.ThenBy):
            case nameof(Queryable.OrderBy):
                orderByType = SqlOrderByType.Asc;
                break;
            case nameof(Queryable.OrderByDescending):
            case nameof(Queryable.ThenByDescending):
                orderByType = SqlOrderByType.Desc;
                break;
            default:
                orderByType = SqlOrderByType.Asc;
                break;
        }

        SqlOrderByItemExpression sqlOrderByItemExpression = new SqlOrderByItemExpression()
        {
            Body = bodySqlExpression,
            OrderByType = orderByType
        };
        if (result.Query is SqlSelectQueryExpression sqlSelectQueryExpression)
        {
            sqlSelectQueryExpression.OrderBy.Items.Add(sqlOrderByItemExpression);
        }

        return GetWrapperExpression(sqlOrderByItemExpression);
    }
    public virtual Expression VisitSelectCall(MethodCallExpression selectCall)
    {
        this.Visit(selectCall.Arguments[0]);
        var lambda = (LambdaExpression)this.StripQuotes(selectCall.Arguments[1]);
        var bodyResult = this.Visit(lambda.Body);
        if (bodyResult is WrapperExpression { SqlExpression: SqlSelectItemExpression sqlSelectItemExpression })
        {
            AppendSqlSelectItem(sqlSelectItemExpression);
        }
        else if (lambda.Body is ParameterExpression && bodyResult is WrapperExpression { SqlExpression: SqlIdentifierExpression sqlIdentifierExpression })
        {
            var tempSqlSelectItemExpression = new SqlSelectItemExpression()
            {
                Body = new SqlIdentifierExpression()
                {
                    Value = "*"
                }
            };
            AppendSqlSelectItem(tempSqlSelectItemExpression);
        }
        return selectCall;
    }

    private void AppendSqlSelectItem(SqlSelectItemExpression sqlSelectItemExpression)
    {
        if (result.Query is SqlSelectQueryExpression sqlSelectQueryExpression)
        {
            sqlSelectQueryExpression.Columns.Add(sqlSelectItemExpression);
        }
    }

    public virtual Expression VisitGroupByCall(MethodCallExpression groupByCall)
    {
        var sourceExpression = this.Visit(groupByCall.Arguments[0]);
        var lambda = (LambdaExpression)this.StripQuotes(groupByCall.Arguments[1]);
        var bodyExpression = this.Visit(lambda.Body);
        if (CheckIsHandled(bodyExpression))
        {
            return bodyExpression;
        }
        var sourceSqlExpression = GetSqlExpression(sourceExpression);
        var bodySqlExpression = GetSqlExpression(bodyExpression);
        if (result.Query is SqlSelectQueryExpression sqlSelectQueryExpression)
        {
            _lastGroupByExpressions.Add(bodySqlExpression);
            sqlSelectQueryExpression.GroupBy.Items.Add(bodySqlExpression);
        }

        return GetWrapperExpression(bodySqlExpression); 
    }

    private bool CheckIsHandled(Expression expression)
    {
        if (expression is WrapperExpression wrapperExpression)
        {
            return wrapperExpression.IsHandled;
        }

        throw new NotSupportedException(nameof(expression));
    }

    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        if (unaryExpression.NodeType == ExpressionType.Convert)
        {
            return Visit(unaryExpression.Operand);
        }

        //兼容active?可空类型=1这种情况
        if (unaryExpression.NodeType == ExpressionType.Convert && unaryExpression.Operand is ConstantExpression constantExpression)
        {
            return constantExpression;
        }

        var operatorString = nodeTypeMappings[unaryExpression.NodeType];


        var operand = unaryExpression.Operand;
        var middleResult = this.Visit(operand);

        if (middleResult is WrapperExpression wrapperExpression)
        {
            SqlExpression body = wrapperExpression.SqlExpression;
            if (unaryExpression.NodeType == ExpressionType.Not)
            {
                if (wrapperExpression.PropertyType == typeof(bool))
                {
                    body = new SqlBinaryExpression()
                    {
                        Left = wrapperExpression.SqlExpression,
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlNumberExpression()
                        {
                            Value = 1m
                        }
                    };
                }

                var result = new SqlNotExpression()
                {
                    Body = body
                };
                return GetWrapperExpression(result);
            }
            else
            {
                throw new NotSupportedException("");
            }
        }
        else
        {
            throw new NotSupportedException(nameof(unaryExpression));
        }

    }

    protected override Expression VisitConstant(ConstantExpression constant)
    {
        if (constant.Value is IQueryable queryable)
        {
            var tableName = DbQueryUtil.GetTableName(queryable.ElementType);
            var tableAlias = GetTableAlias();
            tableNameAliasMapping.TryAdd(tableName, tableAlias);
            if (databaseUnit.TableNameMapping != null)
            {
                tableName = databaseUnit.TableNameMapping(tableName);
            }
            var table = new SqlTableExpression()
            {
                Name = AppendQualifier(new SqlIdentifierExpression()
                {
                    Value = tableName
                }),
                Alias = AppendQualifier(new SqlIdentifierExpression()
                {
                    Value = tableAlias
                }),
            };

            result.Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>(),
                From = table,
                OrderBy = new SqlOrderByExpression(),
                GroupBy = new SqlGroupByExpression()
            };
            return GetWrapperExpression(result);
        }
        else if (constant.Value is string strValue)
        {
            if (MethodName == nameof(Queryable.Select))
            {
                var sqlSelectItemExpression = new SqlSelectItemExpression()
                {
                    Body = new SqlStringExpression()
                    {
                        Value = strValue
                    }
                };
                return GetWrapperExpression(sqlSelectItemExpression);
            }
            else if (MethodName == nameof(QueryableMethodsExtension.OrWhere) || MethodName == nameof(Queryable.Where) || MethodName == nameof(Queryable.FirstOrDefault) || MethodName == nameof(Queryable.First) || MethodName == nameof(Queryable.Count))
            {
                var parameterAlias = GetParameterAlias();
                this.parameters.Add(parameterAlias, strValue);
                return GetWrapperExpression(new SqlVariableExpression()
                {
                    Name = parameterAlias,
                    Prefix = prefix
                });
               
            }
        }
        else if (constant.Value.IsNumeric())
        {
            return GetWrapperExpression(new SqlNumberExpression()
            {
                Value = Convert.ToDecimal(constant.Value)
            });
          
        }
        else if (constant.Value is bool boolValue)
        {
            return GetWrapperExpression(new SqlBoolExpression()
            {
                Value = boolValue
            });
           
        }
        return base.VisitConstant(constant);
    }

    protected override Expression VisitParameter(ParameterExpression param)
    {
        var tableName = DbQueryUtil.GetTableName(param.Type);
        var tableExpression = AppendQualifier(new SqlIdentifierExpression()
        {
            Value = tableName
        });

        return GetWrapperExpression(tableExpression);
  
    }

    private object GetValue(Expression member)
    {
        var objectMember = Expression.Convert(member, typeof(object));
        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
        var getter = getterLambda.Compile();
        var result = getter();
        return result;
    }

    private Expression GetConstExpression(Expression member)
    {
        var result = GetValue(member);
        var tempR = new SqlExpression();
        if (result.IsNumeric())
        {
            tempR = new SqlNumberExpression()
            {
                Value = (decimal)result
            };
        }else if (result is bool boolValue)
        {
            tempR = new SqlBoolExpression()
            {
                Value = boolValue
            };
        }
        else
        {
            var parameterName = GetParameterAlias();
            tempR = new SqlVariableExpression()
            {
                Prefix = prefix,
                Name = parameterName
            };

            this.parameters.Add(parameterName, result);
        }

        return GetWrapperExpression(tempR);
    
    }

    /// <summary>
    /// Add a wildcard like
    /// 添加like的通配符
    /// </summary>
    /// <returns></returns>
    private void AddLikeWildcards(Expression expression, string likeLeft = "", string likeRight = "")
    {
        var sqlExpression = GetSqlExpression(expression);
        if (sqlExpression is SqlVariableExpression sqlVariableExpression && this.parameters.GetParamInfos[sqlVariableExpression.Name].Value is string str && (likeLeft.HasText() || likeRight.HasText()))
        {
            this.parameters.GetParamInfos[sqlVariableExpression.Name].Value = likeLeft + str + likeRight;
        }
    }

    private Expression GetWrapperExpression(SqlExpression sqlExpression,Type propertyType=null)
    {
       return new WrapperExpression() { SqlExpression = sqlExpression,PropertyType = propertyType };
    }

    protected override Expression VisitMember(MemberExpression memberExpression)
    {
        //区分groupBy,单独提取列名
        if (MethodName == nameof(Queryable.Select) && LastMethodName == nameof(Queryable.GroupBy) && memberExpression.Expression is ParameterExpression groupByParameterExpression && groupByParameterExpression.Type.IsGenericType && groupByParameterExpression.Type.GetGenericTypeDefinition().FullName == "System.Linq.IGrouping`2")
        {
            if (memberExpression.Member.Name == "Key")
            {
                var sqlSelectItemExpression = new SqlSelectItemExpression()
                {
                    Body = _lastGroupByExpressions.Last()
                };
                return GetWrapperExpression(sqlSelectItemExpression);
            }
        }

        if (MethodName == nameof(Queryable.Select))
        {
            //如果是可以直接获取值得
            if (memberExpression.Expression is MemberExpression parentExpression)
            {
                var value = GetValue(memberExpression);
                var tempR = new SqlExpression();
                if (value is string str)
                {
                    tempR = new SqlStringExpression()
                    {
                        Value = str
                    };
                }
                else if (value.GetType().IsNumberType())
                {
                    tempR = new SqlNumberExpression()
                    {
                        Value = (decimal)value
                    };
                }
                var sqlSelectItemExpression = new SqlSelectItemExpression()
                {
                    Body = tempR
                };
                return GetWrapperExpression(sqlSelectItemExpression);
            
            }
            else
            {
                var propertyInfo = memberExpression.Member;
                Expression table = null;
                if (memberExpression.Expression != null)
                {
                    table = this.Visit(memberExpression.Expression);
                }
                var columnName = DbQueryUtil.GetColumnName(propertyInfo);
                SqlExpression body = null;
                if (memberExpression.Expression is ParameterExpression && table is WrapperExpression { SqlExpression: SqlIdentifierExpression tableIdentifierExpression } wrapperExpression)
                {
                    body = new SqlPropertyExpression()
                    {
                        Name = AppendQualifier(new SqlIdentifierExpression()
                        {
                            Value = columnName
                        }),
                        Table = AppendQualifier(new SqlIdentifierExpression()
                        {
                            Value = tableNameAliasMapping[tableIdentifierExpression.Value]
                        }),
                    };
                }
                else
                {
                    body = AppendQualifier(new SqlIdentifierExpression()
                    {
                        Value = columnName
                    });
                }
                var sqlSelectItemExpression = new SqlSelectItemExpression()
                {
                    Body = body
                };
                return GetWrapperExpression(sqlSelectItemExpression);
             
            }
        }
        else if (MethodName == nameof(RepositoryMethod.JoinOn) || MethodName == nameof(RepositoryMethod.MultiQueryWhere) || MethodName == nameof(RepositoryMethod.MultiQueryOrderBy) || MethodName == nameof(RepositoryMethod.MultiSelect))
        {
            //解析静态值,例如dto里的参数，dto.Name
            if (GetNumberOfMemberExpressionLayers(memberExpression) > 0 && GetMemberExpressionLastExpression(memberExpression) is ConstantExpression constantExpression)
            {
                var value = GetConstExpression(memberExpression);
                return value;
            }
            //解析类似于it.T1.Name这种
            else if (GetNumberOfMemberExpressionLayers(memberExpression) == 2 && memberExpression.Expression is MemberExpression rightSecondMemberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                var table = new TableExpression(memberExpression.Member.ReflectedType);
                var tableAlias = rightSecondMemberExpression.Member.Name;
                var column = new ColumnExpression(propertyInfo.PropertyType, tableAlias, propertyInfo, 0);
                column.Table = table;
                return column;
            }
            else if (GetNumberOfMemberExpressionLayers(memberExpression) == 3 && memberExpression.Member.Name == "Length" && memberExpression.Member.DeclaringType == typeof(string))
            {
                var firstLayer = GetNestedMemberExpression(memberExpression, 3);
                var secondLayer = GetNestedMemberExpression(memberExpression, 2);
                var table = new TableExpression(memberExpression.Member.ReflectedType);
                var tableAlias = firstLayer.Member.Name;
                var secondLayerPropertyInfo = secondLayer.Member as PropertyInfo;
                var column = new ColumnExpression(secondLayerPropertyInfo.PropertyType, tableAlias, secondLayerPropertyInfo, 0);
                column.FunctionName = "LEN";
                column.Table = table;
                return column;
            }
            //如果是匿名类
            else if (GetNumberOfMemberExpressionLayers(memberExpression) > 0 && GetMemberExpressionLastExpression(memberExpression) is NewExpression newExpression && newExpression.Arguments.Any() && newExpression.Arguments[0] is ConstantExpression)
            {
                var value = GetValue(memberExpression);
                return Expression.Constant(value);
            }
            //只有一层，类似于it.T1这种
            else if (GetNumberOfMemberExpressionLayers(memberExpression) == 1
                     && memberExpression.Expression.Type != null
                     && memberExpression.Expression.Type.Name.Contains("JoinCondition"))
            {
                var tableAlias = memberExpression.Member.Name;
                var table = new TableExpression(memberExpression.Type, tableAlias);
                return new ColumnsExpression(table.Columns, memberExpression.Type);
            }
            else if (IsNullableGetValue(memberExpression))
            {
                return Visit(memberExpression.Expression);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        //if (MethodName == nameof(Queryable.Where) || MethodName == nameof(Queryable.OrderBy) || MethodName == nameof(Queryable.OrderByDescending) || MethodName == nameof(Queryable.GroupBy))
        else
        {
            //如果是可以直接获取值得
            if (memberExpression.Expression is MemberExpression parentExpression)
            {
                //兼容it.name.length>5
                if (memberExpression.Member is PropertyInfo propertyInfo && propertyInfo?.Name == "Length")
                {
                    //获取所有列
                    var middleExpression = this.Visit(parentExpression);
                    var sqlExpression = GetSqlExpression(middleExpression);
                    var result = new SqlFunctionCallExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = lengthName
                        },
                        Arguments = new List<SqlExpression>()
                        {
                            sqlExpression
                        }
                    };
                    return GetWrapperExpression(result);
                  
                }
                else
                {
                    //判断是否为可空类型
                    if (IsNullableGetValue(memberExpression))
                    {
                        var middlExpression = this.Visit(parentExpression);
                        return middlExpression;
                    }
                    else
                    {
                        var value = GetConstExpression(memberExpression);
                        return value;
                    }

                }

            }
            //如果是it.name这种形式
            else if (memberExpression.Expression is ParameterExpression parameterExpression)
            {
                //获取所有列
                var table = this.VisitParameter(parameterExpression);
                var memberInfo = memberExpression.Member;

                var columnName = DbQueryUtil.GetColumnName(memberInfo);
                SqlExpression body = null;
                if (table is WrapperExpression { SqlExpression: SqlIdentifierExpression tableIdentifierExpression } wrapperExpression)
                {
                    body = new SqlPropertyExpression()
                    {
                        Name = AppendQualifier(new SqlIdentifierExpression()
                        {
                            Value = columnName
                        }),
                        Table = AppendQualifier(new SqlIdentifierExpression()
                        {
                            Value = tableNameAliasMapping[tableIdentifierExpression.Value]
                        }),
                    };
                }
                else
                {
                    throw new NotSupportedException("");
                }

                return GetWrapperExpression(body, memberInfo.GetMemberType());

            }
            //如果是constant
            else if (memberExpression.Expression is ConstantExpression constantExpression)
            {
                var value = GetConstExpression(memberExpression);
                return value;
            }
            //如果是匿名类
            else if (memberExpression.Expression is NewExpression newExpression && newExpression.Arguments.Any() && newExpression.Arguments[0] is ConstantExpression)
            {
                var value = GetConstExpression(memberExpression);
                return value;
            }
            //如果是new一个实例，如it.Time == new TestWhereNewDatetime().Time
            else if (memberExpression.Expression is NewExpression newExpression2)
            {
                var value = GetConstExpression(memberExpression);
                return value;
            }
        }

        throw new NotSupportedException(this.MethodName);

    }


    private T AppendQualifier<T>(T t) where T : IQualifierExpression
    {
        t.LeftQualifiers = leftQualifiers;
        t.RightQualifiers = rightQualifiers;
        return t;
    }

    private void AppendSelectItemToResult(SqlSelectItemExpression sqlSelectItemExpression)
    {
        if (result.Query is SqlSelectQueryExpression sqlSelectQueryExpression)
        {
            sqlSelectQueryExpression.Columns.Add(sqlSelectItemExpression);
        }
    }

    /// <summary>
    /// 判断是否可空类型
    /// </summary>
    /// <param name="memberExpression"></param>
    /// <returns></returns>
    private bool IsNullableGetValue(MemberExpression memberExpression)
    {
        return memberExpression.Member != null && memberExpression.Member.Name == "Value" &&
                        memberExpression.Member.DeclaringType != null &&
                        memberExpression.Member.DeclaringType.IsGenericType &&
                        memberExpression.Member.DeclaringType.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
    /// <summary>
    /// 获取嵌套的member的层数
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    private int GetNumberOfMemberExpressionLayers(Expression expression)
    {
        var result = 0;
        if (expression is MemberExpression memberExpression)
        {
            result++;
            result += GetNumberOfMemberExpressionLayers(memberExpression.Expression);
        }

        return result;
    }

    /// <summary>
    /// 获取memberexpression嵌套的最后一个expression,例如it.T1.Name
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    private Expression GetMemberExpressionLastExpression(Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            return GetMemberExpressionLastExpression(memberExpression.Expression);
        }
        else
        {
            return expression;
        }
    }
    /// <summary>
    /// 获取嵌套的指定层数的memberExpression,比如it.T1.Name这种，获取第二层，即.Name这层
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="numberOfLayers">嵌套的层数</param>
    /// <returns></returns>
    private MemberExpression GetNestedMemberExpression(Expression expression, int numberOfLayers, int currentLayer = 0)
    {
        if (expression is MemberExpression memberExpression)
        {
            currentLayer++;
            if (currentLayer == numberOfLayers)
            {
                return memberExpression;
            }
            var next = GetNestedMemberExpression(memberExpression.Expression, numberOfLayers, currentLayer);
            if (next == null)
            {
                throw new NotSupportedException($"can not find {numberOfLayers} memberExpression");
            }

            return next;
        }
        else
        {
            return null;
        }
    }

    private Expression VisitMultiSelectNew(NewExpression newExpression)
    {
        var newColumns = new List<ColumnExpression>();
        for (int i = 0; i < newExpression.Members.Count; i++)
        {
            var memberInfo = newExpression.Members[i];
            if (!(memberInfo is PropertyInfo))
            {
                throw new NotSupportedException("only support T1,T2 etc");
            }
            var argument = newExpression.Arguments[i];
            if (argument is MemberExpression firstMemberExpression && firstMemberExpression.Expression is MemberExpression memberExpression)
            {
                var tempColumnExpression = new ColumnExpression((firstMemberExpression.Member as PropertyInfo).PropertyType, memberExpression.Member.Name,
                    firstMemberExpression.Member, 0);
                tempColumnExpression.ColumnAlias = memberInfo.Name;
                newColumns.Add(tempColumnExpression);
            }
        }

        var result = new ColumnsExpression(newColumns, newExpression.Type);
        return result;
    }

    private Expression VisitMultiSelectMemberInit(MemberInitExpression memberInitExpression)
    {
        var newColumns = new List<ColumnExpression>();

        for (int i = 0; i < memberInitExpression.Bindings.Count; i++)
        {
            var binding = memberInitExpression.Bindings[i];
            var memberInfo = binding.Member;
            if (!(memberInfo is PropertyInfo))
            {
                throw new NotSupportedException("only support T1,T2 etc");
            }

            if (binding is MemberAssignment memberAssignment && memberAssignment.Expression is MemberExpression firstMemberExpression && firstMemberExpression.Expression is MemberExpression memberExpression)
            {
                var tempColumnExpression = new ColumnExpression((firstMemberExpression.Member as PropertyInfo).PropertyType, memberExpression.Member.Name,
                    firstMemberExpression.Member, 0);
                tempColumnExpression.ColumnAlias = DbQueryUtil.GetColumnName((memberInfo as PropertyInfo));
                newColumns.Add(tempColumnExpression);
            }
            else
            {
                throw new NotSupportedException("argument can not parse");
            }
        }

        var result = new ColumnsExpression(newColumns, memberInitExpression.Type);
        return result;
    }

    protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
    {
        var newColumns = new List<ColumnExpression>();

        for (int i = 0; i < memberInitExpression.Bindings.Count; i++)
        {
            var binding = memberInitExpression.Bindings[i];
            var memberInfo = binding.Member;
            if (!(memberInfo is PropertyInfo))
            {
                throw new NotSupportedException(memberInfo.ToString());
            }

            if (binding is MemberAssignment memberAssignment && memberAssignment.Expression is MemberExpression memberExpression)
            {
                var tempColumnExpression = new ColumnExpression((memberExpression.Member as PropertyInfo).PropertyType, memberExpression.Member.Name,
                    memberExpression.Member, 0);
                tempColumnExpression.ColumnAlias = DbQueryUtil.GetColumnName((memberInfo as PropertyInfo));
                newColumns.Add(tempColumnExpression);
            }
            else
            {
                throw new NotSupportedException("argument can not parse");
            }
        }

        var result = new ColumnsExpression(newColumns, memberInitExpression.Type);
        return result;
    }

    protected override Expression VisitNew(NewExpression newExpression)
    {
        //it.Time == new DateTime(2024, 7, 23)
        if (newExpression.Members == null)
        {
            var value = GetConstExpression(newExpression);
            return value;
        }

        for (int i = 0; i < newExpression.Members.Count; i++)
        {
            var memberInfo = newExpression.Members[i];
            var argument = newExpression.Arguments[i];
            var middleResult = this.Visit(argument);
            var bodySqlExpression = GetSqlExpression(middleResult);
            if (MethodName == nameof(Queryable.Select))
            {
                if (bodySqlExpression is SqlSelectItemExpression sqlSelectItemExpression)
                {

                }
                else
                {
                    sqlSelectItemExpression = new SqlSelectItemExpression()
                    {
                        Body = bodySqlExpression,
                    };
                }

                sqlSelectItemExpression.Alias = new SqlIdentifierExpression()
                {
                    LeftQualifiers = leftQualifiers,
                    Value = DbQueryUtil.GetColumnName(memberInfo),
                    RightQualifiers = rightQualifiers
                };
                AppendSelectItemToResult(sqlSelectItemExpression);
            }
            else if (MethodName == nameof(Queryable.GroupBy))
            {
                if (result.Query is SqlSelectQueryExpression sqlSelectQueryExpression)
                {
                    _lastGroupByExpressions.Add(bodySqlExpression);
                    sqlSelectQueryExpression.GroupBy.Items.Add(bodySqlExpression);
                }
            }
            else
            {
                throw new NotSupportedException("parse error");
            }
        }

        return new WrapperExpression() { IsHandled = true };
    }

}