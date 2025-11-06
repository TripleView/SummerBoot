using SqlParser.Net;
using SqlParser.Net.Ast;
using SqlParser.Net.Ast.Expression;
using SummerBoot.Core;
using SummerBoot.Repository.Core;
using SummerBoot.Repository.ExpressionParser.Base;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.ExpressionParser.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

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
                leftQualifiers = "\"";
                rightQualifiers = "\"";
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
    private Dictionary<string, TableInfo> tableNameAliasMapping = new Dictionary<string, TableInfo>();

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

    private int index = -1;

    #region 表名参数名生成管理

    private int tableIndex = -1;
    private int parameterIndex = -1;

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
        index++;
        var tempIndex = index;
        var result = base.Visit(exp);

        if (tempIndex == 0)
        {
            if (result is WrapperExpression wrapperExpression)
            {
                //在没有选择字段的情况下，默认所有字段
                AddDefaultColumns(result);
                wrapperExpression.Parameters = this.parameters;
            }
        }

        return result;
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        // 仅访问 lambda 的体（Body），不访问参数（Parameters）
        var newBody = Visit(node.Body);
        return newBody; // 保留原参数
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
                return result6;
            case nameof(Queryable.Distinct):
                //针对group by里的count单独处理
                if (methodName == nameof(Queryable.Count) && LastMethodName == nameof(Queryable.GroupBy))
                {
                    break;
                }

                methodCallStack.Push(methodName);
                var result5 = this.VisitDistinctCall(node);
                var lastMethodCallName5 = methodCallStack.Pop();
                lastMethodCalls.Add(lastMethodCallName5);
                return result5;
            case nameof(Queryable.Count):
                //针对group by里的count单独处理
                if (methodName == nameof(Queryable.Count) && LastMethodName == nameof(Queryable.GroupBy))
                {
                    break;
                }
                methodCallStack.Push(methodName);
                var result9 = this.VisitCount(node);
                var lastMethodCallName9 = methodCallStack.Pop();
                lastMethodCalls.Add(lastMethodCallName9);
                return result9;
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

        if (method.DeclaringType == null)
        {
            throw new NotSupportedException(methodName);
        }

        var d = DefaultFunctionCall.Contains == method;
        if (databaseUnit.SqlFunctionMappings.TryGetValue(method, out var callbackFunc))
        {
            var methodBodyExpression = this.Visit(node.Object);
            var methodBody = GetSqlExpression(methodBodyExpression);
            var parameterSqlExpressions = new List<SqlExpression>();
            if (node.Arguments.Count > 0)
            {
                foreach (var nodeArgument in node.Arguments)
                {
                    var nodeArgumentExpression = this.Visit(nodeArgument);
                    var parameterSqlExpression = GetSqlExpression(nodeArgumentExpression);
                    parameterSqlExpression = ChangeSqlStringExpressionToSqlVariableExpression(parameterSqlExpression);

                    parameterSqlExpressions.Add(parameterSqlExpression);
                }
            }

            var callResult = callbackFunc(new FunctionCallInfo()
            {
                Body = methodBody,
                DynamicParameters = this.parameters,
                FunctionParameters = parameterSqlExpressions,
                AddParameter = this.GetSqlVariableExpression
            });

            return GetWrapperExpression(callResult);
        }
        else if (method.DeclaringType == typeof(DateTime))
        {
            var date = this.GetValue(node);
            var r1 = GetSqlVariableExpression(date);
            return GetWrapperExpression(r1);
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
                var sqlVariableExpressions = new List<SqlVariableExpression>();
                foreach (var value in values)
                {
                    var sqlVariableExpression = GetSqlVariableExpression(value);

                    sqlVariableExpressions.Add(sqlVariableExpression);
                    if ((i >= 500 && i % 500 == 0) || i == count)
                    {
                        var targetList = new List<SqlExpression>();
                        targetList.AddRange(sqlVariableExpressions);

                        var result = new SqlInExpression()
                        {
                            Body = body,
                            TargetList = targetList
                        };
                        sqlInExpressions.Add(result);

                        sqlVariableExpressions.Clear();
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
                return GetWrapperExpression(sqlInExpressions.First());
            }
        }

        throw new NotSupportedException(
            $"not support method {methodName},You can call the function AddSqlFunctionMapping in DatabaseUnit to add mapping");
    }

    /// <summary>
    /// Remove parameter reference wrappers from expressions
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

    private SqlExpression ChangeSqlStringExpressionToSqlVariableExpression(SqlExpression sqlExpression)
    {
        if (sqlExpression is SqlStringExpression sqlStringExpression)
        {
            return GetSqlVariableExpression(sqlStringExpression.Value);
        }

        return sqlExpression;
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
            left = ChangeSqlStringExpressionToSqlVariableExpression(left);
            right = ChangeSqlStringExpressionToSqlVariableExpression(right);

            var leftIsNull = left is SqlNullExpression;
            var rightIsNull = right is SqlNullExpression;
            if (leftIsNull || rightIsNull)
            {
                if (sqlBinaryOperator.Equals(SqlBinaryOperator.EqualTo))
                {
                    sqlBinaryOperator = SqlBinaryOperator.Is;
                }
                if (sqlBinaryOperator.Equals(SqlBinaryOperator.NotEqualTo))
                {
                    sqlBinaryOperator = SqlBinaryOperator.IsNot;
                }

                if (leftIsNull)
                {
                    (left, right) = (right, left);
                }
            }
            var tempResult = new SqlBinaryExpression()
            {
                Left = left,
                Right = right,
                Operator = sqlBinaryOperator
            };

            return GetWrapperExpression(tempResult);
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
                    Right = GetSqlNumberExpression(1)
                };
                left.SqlExpression = new SqlBinaryExpression()
                {
                    Left = left.SqlExpression,
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = GetSqlNumberExpression(1)
                };
            }
            //it.HaveChildren && it.Name == "hzp"
            else if (left.PropertyType == typeof(bool) && right.SqlExpression is not SqlBoolExpression)
            {
                left.SqlExpression = new SqlBinaryExpression()
                {
                    Left = left.SqlExpression,
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = GetSqlNumberExpression(1)
                };
            }
            //it.Name == "hzp" && it.HaveChildren 
            else if (right.PropertyType == typeof(bool) && left.SqlExpression is not SqlBoolExpression)
            {
                right.SqlExpression = new SqlBinaryExpression()
                {
                    Left = right.SqlExpression,
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = GetSqlNumberExpression(1)
                };
            }
            //false ==  it.HaveChildren 
            else if (right.PropertyType == typeof(bool) && left.SqlExpression is SqlBoolExpression sqlBoolExpression)
            {
                left.SqlExpression = GetSqlNumberExpression(sqlBoolExpression.Value ? 1 : 0);
            }
            //it.HaveChildren == false 
            else if (left.PropertyType == typeof(bool) && right.SqlExpression is SqlBoolExpression sqlBoolExpression2)
            {
                right.SqlExpression = GetSqlNumberExpression(sqlBoolExpression2.Value ? 1 : 0);
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
                Left = GetSqlNumberExpression(1),
                Operator = SqlBinaryOperator.EqualTo,
                Right = GetSqlNumberExpression(sqlBoolExpression4.Value ? 1 : 0),
            };
        }
    }



    public virtual Expression VisitFirstOrDefault(MethodCallExpression firstOrDefaultCall)
    {

        var sourceExpression = this.Visit(firstOrDefaultCall.Arguments[0]);

        var sqlExpression = GetSqlExpression(sourceExpression);
        if (firstOrDefaultCall.Arguments.Count == 2)
        {
            var lambda = (LambdaExpression)this.StripQuotes(firstOrDefaultCall.Arguments[1]);
            var where = GetSqlExpression(this.Visit(lambda.Body));
            MergeWhereExpression(sourceExpression, where);
        }
        AddDefaultColumns(sqlExpression);
        var r1 = ParsingPage(sourceExpression, 0, 1);

        return r1;
    }

    private void MergeWhereExpression(Expression sourceExpression, SqlExpression where)
    {
        var sqlSelectQueryExpression = GetSqlSelectQueryExpression(sourceExpression);
        if (sqlSelectQueryExpression.Where != null)
        {
            sqlSelectQueryExpression.Where = new SqlBinaryExpression()
            {
                Left = sqlSelectQueryExpression.Where,
                Operator = SqlBinaryOperator.And,
                Right = where
            };
        }
        else
        {
            sqlSelectQueryExpression.Where = where;
        }
    }

    private AddParentSqlSelectExpressionResult AddParentSqlSelectExpression(SqlSelectExpression sqlSelectExpression, Action<SqlSelectQueryExpression> childrenSqlSelectExpression)
    {
        if (sqlSelectExpression.Query is SqlSelectQueryExpression sqlSelectQueryExpression)
        {
            var tableAlias = GetTableAlias();
            var tableAliasSqlIdentifierExpression = GetSqlIdentifierExpression(tableAlias);
            var columns = sqlSelectQueryExpression.Columns.Select(x => x.Clone()).ToList();
            foreach (var sqlSelectItemExpression in columns)
            {
                if (sqlSelectItemExpression.Body is SqlPropertyExpression sqlPropertyExpression)
                {

                    sqlPropertyExpression.Table = tableAliasSqlIdentifierExpression;
                    sqlPropertyExpression.Name = sqlSelectItemExpression.Alias ?? sqlPropertyExpression.Name;
                }
                else if (sqlSelectItemExpression.Body is SqlIdentifierExpression sqlIdentifierExpression)
                {
                    var tempSqlPropertyExpression = new SqlPropertyExpression()
                    {

                        Table = tableAliasSqlIdentifierExpression
                    };
                    sqlSelectItemExpression.Body = tempSqlPropertyExpression;
                    tempSqlPropertyExpression.Name = sqlSelectItemExpression.Alias ?? sqlIdentifierExpression.Clone();
                }

                sqlSelectItemExpression.Alias = null;
            }

            if (childrenSqlSelectExpression != null)
            {
                childrenSqlSelectExpression(sqlSelectQueryExpression);
            }

            sqlSelectExpression.Alias = tableAliasSqlIdentifierExpression;
            var sqlExpression = new SqlSelectExpression()
            {
                DbType = dbType,
                Query = new SqlSelectQueryExpression()
                {
                    Columns = columns,
                    From = sqlSelectExpression
                }
            };
            return new AddParentSqlSelectExpressionResult()
            {
                SqlSelectExpression = sqlExpression,
                TableAlias = tableAlias
            };
        }
        return new AddParentSqlSelectExpressionResult()
        {
            SqlSelectExpression = sqlSelectExpression,
            TableAlias = ""
        };
    }


    public virtual Expression ParsingPage(Expression sourceExpression, int offset, int pageSize)
    {
        var sqlExpression = GetSqlExpression(sourceExpression);
        var isRowNumber = databaseUnit.IsSqlServer && databaseUnit.SqlServerVersion < 11 || databaseUnit.IsOracle;

        if (sqlExpression is SqlSelectExpression { Query: SqlSelectQueryExpression sqlSelectQueryExpression } sqlSelectExpression)
        {
            if (isRowNumber)
            {
                var orderBy = new SqlOrderByExpression();
                if (sqlSelectQueryExpression.OrderBy?.Items.Any() == true)
                {
                    orderBy = sqlSelectQueryExpression.OrderBy.Clone();
                    sqlSelectQueryExpression.OrderBy = null;
                }
                else if (sqlSelectQueryExpression.From is SqlTableExpression sqlTableExpression && tableNameAliasMapping.TryGetValue(sqlTableExpression.Name.Value, out var sqlInfo))
                {
                    var orderByColumn = sqlInfo.Columns.FirstOrDefault(x => x.IsKey) ?? sqlInfo.Columns.First();
                    orderBy = new SqlOrderByExpression()
                    {
                        Items = new List<SqlOrderByItemExpression>()
                            {
                                new SqlOrderByItemExpression()
                                {
                                    Body = GetSqlPropertyExpression(sqlInfo.TableAlias, orderByColumn.Name)
                                },
                            },
                    };
                }

                var tempResult = AddParentSqlSelectExpression(sqlSelectExpression,
                    x =>
                    {
                        x.Columns.Add(new SqlSelectItemExpression()
                        {
                            Body = new SqlFunctionCallExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "ROW_NUMBER"
                                },
                                Over = new SqlOverExpression()
                                {
                                    OrderBy = orderBy
                                },
                            },
                            Alias = GetSqlIdentifierExpression("sbRowNo")
                        });
                    });

                sqlSelectExpression = tempResult.SqlSelectExpression;
                var pageColumnExpression = GetSqlPropertyExpression(tempResult.TableAlias, "sbRowNo");

                var where = new SqlBinaryExpression()
                {
                    Left = new SqlBinaryExpression()
                    {
                        Left = pageColumnExpression,
                        Operator = SqlBinaryOperator.GreaterThen,
                        Right = GetSqlVariableExpression(offset)
                    },
                    Operator = SqlBinaryOperator.And,
                    Right = new SqlBinaryExpression()
                    {
                        Left = pageColumnExpression,
                        Operator = SqlBinaryOperator.LessThenOrEqualTo,
                        Right = GetSqlVariableExpression(offset + pageSize)
                    },
                };
                if (sqlSelectExpression.Query is SqlSelectQueryExpression tempSqlSelectQueryExpression)
                {
                    tempSqlSelectQueryExpression.Where = where;
                }
                return GetWrapperExpression(sqlSelectExpression);
            }

            if (pageSize <= 0)
            {
                pageSize = int.MaxValue;
            }

            sqlSelectQueryExpression.Limit = new SqlLimitExpression()
            {
                Offset = GetSqlVariableExpression(offset),
                RowCount = GetSqlVariableExpression(pageSize)
            };

        }

        return GetWrapperExpression(sqlExpression);
    }


    private SqlPropertyExpression GetSqlPropertyExpression(string tableName, string propertyName)
    {
        return new SqlPropertyExpression()
        {
            Name = GetSqlIdentifierExpression(propertyName),
            Table = GetSqlIdentifierExpression(tableName),
            DbType = dbType
        };
    }

    private SqlIdentifierExpression GetSqlIdentifierExpression(string value)
    {
        return AppendQualifier(new SqlIdentifierExpression()
        {
            Value = value,
            DbType = dbType
        });
    }

    private SqlVariableExpression GetSqlVariableExpression(object value)
    {
        var parameterAlias = GetParameterAlias();
        this.parameters.Add(parameterAlias, value);
        return new SqlVariableExpression()
        {
            Name = parameterAlias,
            Prefix = prefix,
            DbType = dbType
        };
    }

    public virtual Expression VisitDistinctCall(MethodCallExpression firstOrDefaultCall)
    {
        var methodName = firstOrDefaultCall.Method.Name;
        var sourceExpression = this.Visit(firstOrDefaultCall.Arguments[0]);
        var sqlSourceExpression = GetSqlExpression(sourceExpression);
        if (sqlSourceExpression is SqlSelectExpression { Query: SqlSelectQueryExpression sqlSelectQueryExpression } sqlSelectExpression)
        {
            sqlSelectQueryExpression.ResultSetReturnOption = SqlResultSetReturnOption.Distinct;

            return sourceExpression;
        }
        else
        {
            throw new NotSupportedException(nameof(firstOrDefaultCall));
        }
    }

    public virtual Expression VisitCount(MethodCallExpression countCall)
    {
        var sourceExpression = this.Visit(countCall.Arguments[0]);
        var sqlSourceExpression = GetSqlExpression(sourceExpression);
        SqlExpression where = null;
        if (countCall.Arguments.Count == 2)
        {
            var lambda = (LambdaExpression)this.StripQuotes(countCall.Arguments[1]);
            var whereExpression = this.Visit(lambda.Body);
            where = GetSqlExpression(whereExpression);
        }
        if (sqlSourceExpression is SqlSelectExpression { Query: SqlSelectQueryExpression sqlSelectQueryExpression } sqlSelectExpression)
        {
            sqlSelectQueryExpression.Columns.Clear();
            sqlSelectQueryExpression.Columns.Add(new SqlSelectItemExpression()
            {
                Body = new SqlFunctionCallExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "count"
                    },
                    Arguments = new List<SqlExpression>()
                   {
                       new SqlAllColumnExpression()
                   }
                }
            });
            sqlSelectQueryExpression.Where = where;
            return sourceExpression;
        }
        else
        {
            throw new NotSupportedException(nameof(countCall));
        }
    }

    public virtual Expression VisitMaxMinSumAvgCall(MethodCallExpression maxMinCall)
    {
        return null;
        //var methodName = maxMinCall.Method.Name;

        //var sourceExpression = this.Visit(maxMinCall.Arguments[0]);
        //ColumnExpression column = null;
        //if (maxMinCall.Arguments.Count == 2)
        //{
        //    var lambda = (LambdaExpression)this.StripQuotes(maxMinCall.Arguments[1]);
        //    column = this.Visit(lambda.Body) as ColumnExpression;
        //}
        //else
        //{
        //    throw new NotSupportedException(methodName);
        //}

        //column.FunctionName = methodName;
        //if (methodName == nameof(Queryable.Average))
        //{
        //    column.FunctionName = "AVG";
        //}

        //if (sourceExpression is TableExpression table)
        //{
        //    var result = new SelectExpression(null, "", table.Columns, table);

        //    result.Columns.Clear();

        //    result.Columns.Add(column);

        //    return result;
        //}
        //else if (sourceExpression is SelectExpression selectExpression)
        //{
        //    selectExpression.Columns.Clear();
        //    selectExpression.Columns.Add(column);
        //    return selectExpression;
        //}
        //else
        //{
        //    throw new NotSupportedException(methodName);
        //}
    }

    public virtual Expression VisitSkipTakeCall(MethodCallExpression skipTakeExpression)
    {
        var methodName = skipTakeExpression.Method.Name;
        var arg0 = skipTakeExpression.Arguments[0];
        Expression sourceExpression = null;
        var offset = 0;
        var pageSize = -1;
        if (methodName == nameof(Queryable.Take))
        {
            var takeSqlExpression = this.Visit(skipTakeExpression.Arguments[1]);
            var pageSizeSqlExpression = GetSqlNumberExpression(takeSqlExpression);
            pageSize = (int)pageSizeSqlExpression.Value;
            if (arg0 is MethodCallExpression { Method: { Name: nameof(Queryable.Skip) } } leftMethodCallExpression)
            {
                sourceExpression = this.Visit(leftMethodCallExpression.Arguments[0]);
                var skipExpression = this.Visit(leftMethodCallExpression.Arguments[1]);
                var offsetSqlNumberExpression = GetSqlNumberExpression(skipExpression);
                offset = (int)offsetSqlNumberExpression.Value;

            }
            else
            {
                sourceExpression = this.Visit(arg0);
            }

        }
        else if (methodName == nameof(Queryable.Skip))
        {
            sourceExpression = this.Visit(arg0);
            var skipExpression = this.Visit(skipTakeExpression.Arguments[1]);
            var offsetSqlNumberExpression = GetSqlNumberExpression(skipExpression);
            offset = (int)offsetSqlNumberExpression.Value;
        }
        AddDefaultColumns(sourceExpression);
        var r1 = ParsingPage(sourceExpression, offset, pageSize);

        return r1;
    }

    public virtual Expression VisitWhereCall(MethodCallExpression whereCall)
    {
        if (MethodName == nameof(Queryable.GroupBy))
        {
            return whereCall;
        }
        else
        {
            var tempResult = this.Visit(whereCall.Arguments[0]);
            var lambda = (LambdaExpression)this.StripQuotes(whereCall.Arguments[1]);
            var bodyExpression = this.Visit(lambda.Body);
            if (bodyExpression is WrapperExpression tempBodyExpression)
            {
                ConvertBooleanValueToSqlBinaryExpression(tempBodyExpression);
                var sqlSelectQueryExpression = GetSqlSelectQueryExpression(tempResult);
                sqlSelectQueryExpression.Where = GetSqlExpression(tempBodyExpression);
            }

            return tempResult;
        }

    }

    public virtual Expression VisitOrderByCall(MethodCallExpression whereCall)
    {
        var sourceExpression = this.Visit(whereCall.Arguments[0]);
        var lambda = (LambdaExpression)this.StripQuotes(whereCall.Arguments[1]);
        var bodyExpression = this.Visit(lambda.Body);
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
        var sourceSqlExpression = GetSqlSelectQueryExpression(sourceExpression);
        sourceSqlExpression.OrderBy.Items.Add(sqlOrderByItemExpression);

        return sourceExpression;
    }
    public virtual Expression VisitSelectCall(MethodCallExpression selectCall)
    {
        var sourceExpression = this.Visit(selectCall.Arguments[0]);
        var lambda = (LambdaExpression)this.StripQuotes(selectCall.Arguments[1]);
        var bodyResult = this.Visit(lambda.Body);
        var sqlSelectItemExpressions = new List<SqlSelectItemExpression>();
        if (lambda.Body is ParameterExpression && bodyResult is WrapperExpression { SqlExpression: SqlIdentifierExpression sqlIdentifierExpression })
        {
            sqlSelectItemExpressions.Add(new SqlSelectItemExpression()
            {
                Body = new SqlIdentifierExpression()
                {
                    Value = "*"
                }
            });
        }
        else if (bodyResult is WrapperExpression { SqlExpression: SqlPropertyExpression tempSqlPropertyExpression })
        {
            sqlSelectItemExpressions.Add(new SqlSelectItemExpression() { Body = tempSqlPropertyExpression });
        }
        else if (bodyResult is WrapperExpression { SqlExpression: SqlIdentifierExpression tempSqlIdentifierExpression })
        {
            sqlSelectItemExpressions.Add(new SqlSelectItemExpression() { Body = tempSqlIdentifierExpression });
        }

        else if (bodyResult is WrapperExpression { SqlExpressions: List<SqlExpression> list })
        {
            sqlSelectItemExpressions.AddRange(list.Select(x => (SqlSelectItemExpression)x).ToList());
        }
        else
        {
            throw new NotSupportedException();
        }

        var sqlSelectQueryExpression = GetSqlSelectQueryExpression(sourceExpression);
        sqlSelectQueryExpression?.Columns.AddRange(sqlSelectItemExpressions);
        return sourceExpression;
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
        var sourceSqlExpression = GetSqlSelectQueryExpression(sourceExpression);
        if (bodyExpression is WrapperExpression wrapperExpression)
        {
            if (wrapperExpression.SqlExpression != null)
            {
                var bodySqlExpression = GetSqlExpression(bodyExpression);
                _lastGroupByExpressions.Add(bodySqlExpression);
                sourceSqlExpression?.GroupBy.Items.Add(bodySqlExpression);
            }
            else if (wrapperExpression.SqlExpressions.HasValue())
            {
                _lastGroupByExpressions.AddRange(wrapperExpression.SqlExpressions);
                sourceSqlExpression?.GroupBy.Items.AddRange(wrapperExpression.SqlExpressions);
            }
        }



        return sourceExpression;
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
                        Right = GetSqlNumberExpression(1)
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
            var tableAlias = GetTableAlias();
            var tableInfo = new TableInfo(queryable.ElementType)
            {
                TableAlias = tableAlias
            };
            var tableName = tableInfo.Name;
            tableNameAliasMapping.TryAdd(tableName, tableInfo);
            if (databaseUnit.TableNameMapping != null)
            {
                tableName = databaseUnit.TableNameMapping(tableName);
            }
            var table = new SqlTableExpression()
            {
                DbType = dbType,
                Name = GetSqlIdentifierExpression(tableName),
                Alias = GetSqlIdentifierExpression(tableAlias)
            };

            var r1 = new SqlSelectExpression()
            {
                DbType = dbType,
                Query = new SqlSelectQueryExpression()
                {
                    DbType = dbType,
                    Columns = new List<SqlSelectItemExpression>(),
                    From = table,
                }
            };

            return GetWrapperExpression(r1);
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
                var r1 = GetSqlVariableExpression(strValue);

                return GetWrapperExpression(r1);

            }
        }
        else if (constant.Value.IsNumeric())
        {
            return GetWrapperExpression(GetSqlNumberExpression(Convert.ToDecimal(constant.Value)));

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
        var tableExpression = GetSqlIdentifierExpression(tableName);
        return GetWrapperExpression(tableExpression);
    }

    private object GetValue(Expression member)
    {
        var objectMember = Expression.Convert(member, typeof(object));
        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
        var getter = getterLambda.Compile();
        var r = getter();
        return r;
    }

    private Expression GetConstExpression(Expression member)
    {
        var result = GetValue(member);
        var tempR = new SqlExpression();
        if (result.IsNumeric())
        {
            tempR = GetSqlNumberExpression((decimal)result);
        }
        else if (result is bool boolValue)
        {
            tempR = new SqlBoolExpression()
            {
                Value = boolValue
            };
        }
        else if (result is string str)
        {
            tempR = new SqlStringExpression()
            {
                Value = str
            };
        }
        else
        {
            tempR = GetSqlVariableExpression(result);
        }

        return GetWrapperExpression(tempR);

    }



    private Expression GetWrapperExpression(SqlExpression sqlExpression, Type propertyType = null)
    {
        return new WrapperExpression() { SqlExpression = sqlExpression, PropertyType = propertyType };
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

        //
        if (MethodName == nameof(Queryable.Select) && false)
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
                    body = GetSqlPropertyExpression(tableNameAliasMapping[tableIdentifierExpression.Value].TableAlias,
                        columnName);
                }
                else
                {
                    body = GetSqlIdentifierExpression(columnName);
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
                //var table = new TableExpression(memberExpression.Member.ReflectedType);
                //var tableAlias = rightSecondMemberExpression.Member.Name;
                //var column = new ColumnExpression(propertyInfo.PropertyType, tableAlias, propertyInfo, 0);

                //return column;
                return null;
            }
            else if (GetNumberOfMemberExpressionLayers(memberExpression) == 3 && memberExpression.Member.Name == "Length" && memberExpression.Member.DeclaringType == typeof(string))
            {
                return null;
                //var firstLayer = GetNestedMemberExpression(memberExpression, 3);
                //var secondLayer = GetNestedMemberExpression(memberExpression, 2);
                //var table = new TableExpression(memberExpression.Member.ReflectedType);
                //var tableAlias = firstLayer.Member.Name;
                //var secondLayerPropertyInfo = secondLayer.Member as PropertyInfo;
                //var column = new ColumnExpression(secondLayerPropertyInfo.PropertyType, tableAlias, secondLayerPropertyInfo, 0);
                //column.FunctionName = "LEN";

                //return column;
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
                return null;
                //var tableAlias = memberExpression.Member.Name;
                //var table = new TableExpression(memberExpression.Type, tableAlias);
                //return new ColumnsExpression(table.Columns, memberExpression.Type);
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
                    var r1 = new SqlFunctionCallExpression()
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
                    return GetWrapperExpression(r1);

                }
                else
                {
                    //判断是否为可空类型
                    if (IsNullableGetValue(memberExpression))
                    {
                        var middleExpression = this.Visit(parentExpression);
                        return middleExpression;
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
                    tableNameAliasMapping.TryGetValue(tableIdentifierExpression.Value, out var tempTableInfo);
                    if (tempTableInfo != null)
                    {
                        body = GetSqlPropertyExpression(tempTableInfo.TableAlias,
                            columnName);
                    }
                    else
                    {
                        body = GetSqlIdentifierExpression(columnName);
                    }
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

    protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
    {
        return null;
        //var newColumns = new List<ColumnExpression>();

        //for (int i = 0; i < memberInitExpression.Bindings.Count; i++)
        //{
        //    var binding = memberInitExpression.Bindings[i];
        //    var memberInfo = binding.Member;
        //    if (!(memberInfo is PropertyInfo))
        //    {
        //        throw new NotSupportedException(memberInfo.ToString());
        //    }

        //    if (binding is MemberAssignment memberAssignment && memberAssignment.Expression is MemberExpression memberExpression)
        //    {
        //        var tempColumnExpression = new ColumnExpression((memberExpression.Member as PropertyInfo).PropertyType, memberExpression.Member.Name,
        //            memberExpression.Member, 0);
        //        tempColumnExpression.ColumnAlias = DbQueryUtil.GetColumnName((memberInfo as PropertyInfo));
        //        newColumns.Add(tempColumnExpression);
        //    }
        //    else
        //    {
        //        throw new NotSupportedException("argument can not parse");
        //    }
        //}

        //var result = new ColumnsExpression(newColumns, memberInitExpression.Type);
        //return result;
    }

    protected override Expression VisitNew(NewExpression newExpression)
    {
        //it.Time == new DateTime(2024, 7, 23)
        if (newExpression.Members == null)
        {
            var value = GetConstExpression(newExpression);
            return value;
        }

        var list = new List<SqlExpression>();

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
                //todo 重构
                sqlSelectItemExpression.Alias = GetSqlIdentifierExpression(DbQueryUtil.GetColumnName(memberInfo));
                list.Add(sqlSelectItemExpression);
            }
            else if (MethodName == nameof(Queryable.GroupBy))
            {
                list.Add(bodySqlExpression);
            }
            else
            {
                throw new NotSupportedException("parse error");
            }
        }

        if (list.Any())
        {
            return new WrapperExpression()
            {
                SqlExpressions = list
            };
        }

        return new WrapperExpression() { IsHandled = true };
    }

    private void AddDefaultColumns(Expression expression)
    {
        var sqlExpression = GetSqlExpression(expression);
        AddDefaultColumns(sqlExpression);
    }
    /// <summary>
    /// 增加默认列
    /// </summary>
    private void AddDefaultColumns(SqlExpression sqlExpression)
    {
        if (sqlExpression is SqlSelectExpression { Query: SqlSelectQueryExpression sqlSelectQueryExpression } sqlSelectExpression)
        {
            if (sqlSelectQueryExpression.Columns.Count == 0 && sqlSelectQueryExpression.From is SqlTableExpression sqlTableExpression && tableNameAliasMapping.TryGetValue(sqlTableExpression.Name.Value, out var sqlInfo))
            {
                if (sqlSelectQueryExpression.GroupBy?.Items.HasValue() == true)
                {
                    sqlSelectQueryExpression.OrderBy.Items.AddRange(sqlSelectQueryExpression.GroupBy.Items.Select(x => new SqlOrderByItemExpression()
                    {
                        Body = x
                    }));
                    sqlSelectQueryExpression.GroupBy = null;
                }

                foreach (var sqlInfoColumnInfo in sqlInfo.Columns)
                {
                    sqlSelectQueryExpression.Columns.Add(new SqlSelectItemExpression()
                    {
                        Body = GetSqlPropertyExpression(sqlInfo.TableAlias, sqlInfoColumnInfo.Name),
                        Alias = GetSqlIdentifierExpression(sqlInfoColumnInfo.PropertyName)
                    });

                }
            }
        }
    }

    private SqlExpression GetSqlExpression(Expression expression)
    {
        if (expression is WrapperExpression wrapperExpression)
        {
            wrapperExpression.SqlExpression.DbType = dbType;
            return wrapperExpression.SqlExpression;
        }
        else if (expression is ConstantExpression { Value: null } constantExpression)
        {
            return new SqlNullExpression();
        }

        throw new NotSupportedException(nameof(expression));
    }

    private SqlSelectExpression GetSqlSelectExpression(Expression expression)
    {
        if (expression is WrapperExpression { SqlExpression: SqlSelectExpression { Query: SqlSelectQueryExpression sqlSelectQueryExpression } sqlSelectExpression } wrapperExpression)
        {
            return sqlSelectExpression;
        }

        throw new NotSupportedException(nameof(expression));
    }

    private SqlSelectQueryExpression GetSqlSelectQueryExpression(Expression expression)
    {
        if (expression is WrapperExpression { SqlExpression: SqlSelectExpression { Query: SqlSelectQueryExpression sqlSelectQueryExpression } sqlSelectExpression } wrapperExpression)
        {
            return sqlSelectQueryExpression;
        }
        throw new NotSupportedException(nameof(expression));
    }

    private SqlNumberExpression GetSqlNumberExpression(Expression expression)
    {
        if (expression is WrapperExpression { SqlExpression: SqlNumberExpression sqlNumberExpression } wrapperExpression)
        {
            return sqlNumberExpression;
        }
        throw new NotSupportedException(nameof(expression));
    }

    private SqlNumberExpression GetSqlNumberExpression(decimal value)
    {
        return new SqlNumberExpression()
        {
            Value = value,
            DbType = dbType
        };
    }

    protected TableInfo GetTableInfo(Type type)
    {
        var key = "GetTableInfo:" + type.FullName;
        if (SbUtil.CacheDictionary.TryGetValue(key, out var cacheValue))
        {
            return (TableInfo)cacheValue;
        }

        var r1 = new TableInfo(type);
        SbUtil.CacheDictionary.TryAdd(key, r1);
        return r1;
    }

    public virtual DbQueryResult Insert<T>(T insertEntity)
    {
        var key = "GetSqlInsertExpression:" + typeof(T).FullName;
        var cacheResult = (DbQueryResult)SbUtil.CacheDictionary.GetOrAdd(key, x =>
        {
            var table = this.GetTableInfo(typeof(T));
            var dbType = this.dbType;
            var tableExpression = new SqlTableExpression()
            {
                Name = GetSqlIdentifierExpression(table.Name),
            };
            if (table.Schema.HasText())
            {
                tableExpression.Schema = GetSqlIdentifierExpression(table.Schema);
            }
            var insertExpression = new SqlInsertExpression()
            {
                DbType = dbType,
                Table = tableExpression
            };
            var valueExpressions = new List<SqlExpression>();
            foreach (var column in table.Columns)
            {
                var columnName = column.Name;
                if (column.IsKey && column.IsDatabaseGeneratedIdentity && !databaseUnit.IsDataMigrateMode)
                {
                    continue;
                }
                insertExpression.Columns.Add(GetSqlIdentifierExpression(columnName));

                valueExpressions.Add(new SqlVariableExpression()
                {
                    DbType = dbType,
                    Prefix = this.prefix,
                    Name = columnName
                });
            }
            insertExpression.ValuesList.Add(valueExpressions);

            var result = new DbQueryResult()
            {
                ExecuteSqlExpression = insertExpression,
            };

            var keyColumn = table.Columns.FirstOrDefault(it => it.IsKey && it.IsDatabaseGeneratedIdentity && it.Name.ToLower() == "id");
            if (keyColumn != null)
            {
                if (dbType == DbType.SqlServer)
                {
                    result.LastInsertIdSql = "select SCOPE_IDENTITY() id";
                }

                result.LastInsertIdSql = "";
                result.IdKeyPropertyInfo = keyColumn.Property;
                result.IdName = keyColumn.Name;
            }

            return result;
        });

        cacheResult.DynamicParameters = new DynamicParameters(insertEntity);

        return cacheResult;
    }

    public DbQueryResult Update<T>(T updateEntity)
    {
        var key = "GetSqlUpdateExpression" + typeof(T).FullName;
        var cacheResult = (DbQueryResult)SbUtil.CacheDictionary.GetOrAdd(key, x =>
        {
            var table = this.GetTableInfo(typeof(T));
            var dbType = this.dbType;

            var keyColumns = table.Columns.Where(it => it.IsKey).ToList();

            if (!keyColumns.Any())
            {
                throw new Exception("Please set the primary key");
            }

            var tableExpression = new SqlTableExpression()
            {
                Name = GetSqlIdentifierExpression(table.Name)
            };
            if (table.Schema.HasText())
            {
                tableExpression.Schema = GetSqlIdentifierExpression(table.Schema);
            }
            var updateExpression = new SqlUpdateExpression()
            {
                DbType = dbType,
                Table = tableExpression
            };

            var columns = table.Columns.Where(it => !it.IsKey && !it.IsIgnoreWhenUpdate).ToList();


            foreach (var column in columns)
            {
                var columnName = column.Name;

                updateExpression.Items.Add(new SqlBinaryExpression()
                {
                    Left = GetSqlIdentifierExpression(columnName),
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlVariableExpression()
                    {
                        DbType = dbType,
                        Prefix = this.prefix,
                        Name = columnName
                    }
                });
            }

            var keyList = new List<string>();
            foreach (var column in keyColumns)
            {
                var columnName = column.Name;
                if (column.Property.GetValue(updateEntity) is null)
                {
                    var condition = new SqlBinaryExpression()
                    {
                        Left = GetSqlIdentifierExpression(columnName),
                        Operator = SqlBinaryOperator.Is,
                        Right = new SqlNullExpression()
                        {
                            DbType = dbType
                        }
                    };
                    if (updateExpression.Where == null)
                    {
                        updateExpression.Where = condition;
                    }
                    else
                    {
                        updateExpression.Where =
                            new SqlBinaryExpression()
                            {
                                Left = updateExpression.Where,
                                Operator = SqlBinaryOperator.And,
                                Right = condition
                            };
                    }
                }
                else
                {
                    var condition = new SqlBinaryExpression()
                    {
                        Left = GetSqlIdentifierExpression(columnName),
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlVariableExpression()
                        {
                            DbType = dbType,
                            Prefix = this.prefix,
                            Name = columnName
                        }
                    };
                    if (updateExpression.Where == null)
                    {
                        updateExpression.Where = condition;
                    }
                    else
                    {
                        updateExpression.Where =
                            new SqlBinaryExpression()
                            {
                                Left = updateExpression.Where,
                                Operator = SqlBinaryOperator.And,
                                Right = condition
                            };
                    }
                }
            }

            var result = new DbQueryResult()
            {
                ExecuteSqlExpression = updateExpression
            };

            return result;
        });

        cacheResult.DynamicParameters = new DynamicParameters(updateEntity);
        return cacheResult;
    }

    public DbQueryResult ExecuteUpdate<T>(Expression expression, List<SelectItem<T>> updateItems)
    {
        if (updateItems == null || updateItems.Count == 0)
        {
            throw new ArgumentNullException(nameof(updateItems));
        }
        
        var body = this.Visit(expression);
        var sqlSelectQueryExpression= GetSqlSelectQueryExpression(body);
        var where = sqlSelectQueryExpression.Where;
        if (where != null)
        {

        }

        var updateColumns = new List<SqlIdentifierExpression>();
        foreach (var updateItem in updateItems)
        {
            var tempR = this.Visit(updateItem.Select);
            var column = GetSqlExpression(tempR);
            if (column is SqlIdentifierExpression sqlIdentifierExpression)
            {
                updateColumns.Add(sqlIdentifierExpression);
            }
        }

        var updateColumnKeys = updateColumns.Select(x => x.Value).OrderBy(x => x).StringJoin(";");

        var key = "GetExecuteUpdateSqlUpdateExpression" + typeof(T).FullName + updateColumnKeys;
        var cacheResult = (DbQueryResult)SbUtil.CacheDictionary.GetOrAdd(key, x =>
        {
            var table = this.GetTableInfo(typeof(T));
            var dbType = this.dbType;

           

            var tableExpression = new SqlTableExpression()
            {
                Name = GetSqlIdentifierExpression(table.Name)
            };
            if (table.Schema.HasText())
            {
                tableExpression.Schema = GetSqlIdentifierExpression(table.Schema);
            }
            var updateExpression = new SqlUpdateExpression()
            {
                DbType = dbType,
                Table = tableExpression
            };

            foreach (var column in updateColumns)
            {
                updateExpression.Items.Add(new SqlBinaryExpression()
                {
                    Left = column,
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlVariableExpression()
                    {
                        DbType = dbType,
                        Prefix = this.prefix,
                        Name = column.Value
                    }
                });
            }



            var result = new DbQueryResult()
            {
                ExecuteSqlExpression = updateExpression
            };

            return result;
        });

        return cacheResult;
        //Clear();
        //var table = this.getTableExpression(typeof(T));
        //var tableName = GetSchemaTableName(table.Schema, table.Name);

        //var middleResult = this.Visit(expression);
        //this.FormatWhere(middleResult);
        //var whereSql = _sb.ToString();
        //_sb.Clear();

        //var columnSetValueClauses = new List<string>();

        //foreach (var selectItem in selectItems)
        //{
        //    if (selectItem.Select.Body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
        //    {
        //        var body = unaryExpression.Operand;
        //        var bodyResultExpression = this.Visit(body);
        //        if (bodyResultExpression is ColumnExpression columnExpression)
        //        {
        //            this.VisitColumn(columnExpression);
        //            _sb.Append("=");
        //            _sb.Append(BoxParameter(selectItem.Value, columnExpression.ValueType));
        //            var columnSetValueClause = _sb.ToString();
        //            columnSetValueClauses.Add(columnSetValueClause);
        //            _sb.Clear();
        //        }
        //    }
        //    else if (selectItem.Select.Body is MemberExpression memberExpression)
        //    {
        //        var bodyResultExpression = this.Visit(memberExpression);
        //        if (bodyResultExpression is ColumnExpression columnExpression)
        //        {
        //            this.VisitColumn(columnExpression);
        //            _sb.Append("=");
        //            _sb.Append(BoxParameter(selectItem.Value, columnExpression.ValueType));
        //            var columnSetValueClause = _sb.ToString();
        //            columnSetValueClauses.Add(columnSetValueClause);
        //            _sb.Clear();
        //        }
        //    }
        //    else
        //    {
        //        throw new NotSupportedException("setValue only support one property,like it=>it.name");
        //    }
        //}
        //var setValues = string.Join(",", columnSetValueClauses);

        //var deleteSql = $"update {tableName} set {setValues} where 1=1";
        //if (!string.IsNullOrWhiteSpace(whereSql))
        //{
        //    deleteSql += $" and {whereSql}";
        //}

        //var result = new DbQueryResult()
        //{
        //    Sql = deleteSql.Trim(),
        //    SqlParameters = this.sqlParameters
        //};
        //return result;

    }

}