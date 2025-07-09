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
                break;
            case DatabaseType.Mysql:
                dbType = DbType.MySql;
                leftQualifiers = "`";
                rightQualifiers = "`";
                break;
            case DatabaseType.Oracle:
                dbType = DbType.Oracle;
                leftQualifiers = ":";
                rightQualifiers = ":";
                break;
            case DatabaseType.Sqlite:
                dbType = DbType.Sqlite;
                leftQualifiers = "`";
                rightQualifiers = "`";
                break;
            case DatabaseType.Pgsql:
                dbType = DbType.Pgsql;
                leftQualifiers = "\"";
                rightQualifiers = "\"";

                break;
        }
    }
    private Dictionary<string, ColumnExpression> _lastColumns =
        new Dictionary<string, ColumnExpression>();

    private List<GroupByExpression> _lastGroupByExpressions = new List<GroupByExpression>();

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

    private string leftQualifiers;
    private string rightQualifiers;

    #endregion
    /// <summary>
    /// 上一次处理的方法名称
    /// </summary>
    private string LastMethodName => lastMethodCalls.LastOrDefault();


    #region 表名生成管理

    private int _tableIndex = -1;
    /// <summary>
    /// Get the expression tree parsing result
    /// 获取表达式树解析结果
    /// </summary>
    /// <returns></returns>
    public ExpressionTreeParsingResult GetParsingResult()
    {
        return new ExpressionTreeParsingResult()
        {
            Sql = result.ToSql(dbType).Trim(),
            Parameters = parameters
        };
    }
    /// <summary>
    /// 获取新的查询别名
    /// </summary>
    public string GetNewAlias()
    {
        _tableIndex++;
        return "p" + _tableIndex;
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
            case DbExpressionType.Table:
                return this.VisitTable((TableExpression)exp);
            case DbExpressionType.Column:
                return this.VisitColumn((ColumnExpression)exp);
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
    public virtual Expression VisitTable(TableExpression tableExpression)
    {
        return tableExpression;
    }

    public virtual Expression VisitWhere(WhereExpression whereExpression)
    {
        return whereExpression;
    }
    public virtual Expression VisitColumn(ColumnExpression columnExpression)
    {
        return columnExpression;
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
            case nameof(Queryable.Distinct):
            case nameof(Queryable.First):
            case nameof(Queryable.FirstOrDefault):
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
            var functionName = methodName.ToUpper();
            switch (methodName)
            {
                case nameof(Queryable.Count):
                    var result = new ColumnExpression(null, "", null, 0);
                    result.FunctionName = functionName;
                    return result;

                case nameof(Queryable.Max):
                case nameof(Queryable.Min):
                case nameof(Queryable.Sum):
                case nameof(Queryable.Average):
                    if (functionName == "AVERAGE")
                    {
                        functionName = "AVG";
                    }

                    var lambda = (LambdaExpression)this.StripQuotes(node.Arguments[1]);
                    var value = this.Visit(lambda.Body);
                    if (value is ColumnExpression columnExpression)
                    {
                        columnExpression.FunctionName = functionName;
                        return columnExpression;
                    }
                    else
                    {
                        throw new NotSupportedException(methodName);
                    }
            }
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
                if (objectExpression is ColumnExpression columnExpression && node.Arguments.Count > 0)
                {
                    if (node.Arguments[0] is ConstantExpression constantExpression)
                    {
                        var whereConditionExpression = new WhereConditionExpression(columnExpression, "like", BoxMethodLikeParameter(constantExpression.Value, methodName));
                        return whereConditionExpression;
                    }
                    else if (node.Arguments[0] is MemberExpression memberExpression)
                    {
                        var memberResultExpression = this.Visit(memberExpression);
                        if (memberResultExpression is ConstantExpression constantExpression2)
                        {
                            var whereConditionExpression2 = new WhereConditionExpression(columnExpression, "like", BoxMethodLikeParameter(constantExpression2.Value, methodName));
                            return whereConditionExpression2;
                        }
                        else
                        {
                            throw new NotSupportedException(methodName);
                        }
                    }
                    else
                    {
                        var result = this.Visit(node.Arguments[0]);
                        if (result is ConstantExpression constantExpression2)
                        {
                            var whereConditionExpression2 = new WhereConditionExpression(columnExpression, "like", BoxMethodLikeParameter(constantExpression2.Value, methodName));
                            return whereConditionExpression2;
                        }
                        else
                        {
                            throw new NotSupportedException(methodName);
                        }
                    }

                }
                else
                {
                    throw new NotSupportedException(methodName);
                }

            }
            else if (stringMethodTrimUpperLowerInfoList.Contains(node.Method))
            {
                var objectExpression = this.Visit(node.Object);
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
                if (propertyExpression is ColumnExpression columnExpression)
                {
                    var count = 0;
                    foreach (var value in values)
                    {
                        count++;
                    }
                    //判断是否需要每500次进行拆分
                    if (count > 500)
                    {
                        var tempResult = new List<object>();
                        var conditionExpressions = new List<WhereConditionExpression>();
                        var i = 1;

                        foreach (var value in values)
                        {
                            tempResult.Add(value);
                            if ((i != 1 && i % 500 == 0) || i == count)
                            {
                                var newValue = new List<object>(tempResult);

                                var whereConditionExpression2 = new WhereConditionExpression(columnExpression, "in", newValue)
                                {
                                    ValueType = method.DeclaringType
                                };
                                conditionExpressions.Add(whereConditionExpression2);
                                tempResult.Clear();
                            }

                            i++;
                        }

                        var result = FoldWhereExpression(conditionExpressions);
                        return result;
                    }
                    else
                    {
                        var whereConditionExpression2 = new WhereConditionExpression(columnExpression, "in", values)
                        {
                            ValueType = method.DeclaringType
                        };
                        return whereConditionExpression2;
                    }

                }
                else
                {
                    throw new NotSupportedException(methodName);
                }

            }
            else
            {
                var date = this.GetValue(node);
                return ConstantExpression.Constant(date);
                throw new NotSupportedException(methodName);
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
            var @operator = nodeTypeMappings[binaryExpression.NodeType];
            if (string.IsNullOrWhiteSpace(@operator))
            {
                throw new NotSupportedException(nameof(binaryExpression.NodeType));
            }
            
            var rightExpression = this.Visit(binaryExpression.Right);

            var leftExpression = this.Visit(binaryExpression.Left);
            var result = new SqlBinaryExpression()
            {
                Left = leftExpression
            }
            if (true)
            {

            }
            else
            {
                throw new NotSupportedException(MethodName);
            }
            return result;

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
            var tempVisitResult = this.Visit(whereCall.Arguments[0]);
            var lambda = (LambdaExpression)this.StripQuotes(whereCall.Arguments[1]);
            var bodyExpression = this.Visit(lambda.Body);

            TableExpression source = null;
            SelectExpression selectExpression = null;
            if (tempVisitResult is TableExpression tempTableExpression)
            {
                source = tempTableExpression;
            }
            else if (tempVisitResult is SelectExpression tempSelectExpression)
            {
                selectExpression = tempSelectExpression;
            }
            else
            {
                throw new NotSupportedException(tempVisitResult?.NodeType.ToString());
            }

            //兼容it.isHandsome这种单true的条件
            if (bodyExpression is ColumnExpression columnExpression && columnExpression.Type == typeof(bool))
            {
                var whereConditionExpression = new WhereConditionExpression(columnExpression, "=", 1);
                selectExpression = CombineSelectExpression(source, whereConditionExpression, selectExpression, @operator);

                return selectExpression;

            }
            else if (bodyExpression is WhereExpression whereExpression)
            {
                selectExpression = CombineSelectExpression(source, whereExpression, selectExpression, @operator);
                return selectExpression;
            }
            //兼容It=>true这种
            else if (bodyExpression is ConstantExpression constantExpression && constantExpression.Type == typeof(bool))
            {
                var value = (bool)constantExpression.Value;
                var whereTrueFalseValueCondition = new WhereTrueFalseValueConditionExpression(value);
                selectExpression = CombineSelectExpression(source, whereTrueFalseValueCondition, selectExpression, @operator);
                return selectExpression;
            }
            else
            {
                throw new NotSupportedException(nameof(bodyExpression));
            }
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

        if (sourceExpression is TableExpression table)
        {
            if (bodyExpression is ColumnExpression columnExpression)
            {
                OrderByType orderByType;
                switch (whereCall.Method.Name)
                {
                    case nameof(Queryable.ThenBy):
                    case nameof(Queryable.OrderBy):
                        orderByType = OrderByType.Asc;
                        break;
                    case nameof(Queryable.OrderByDescending):
                    case nameof(Queryable.ThenByDescending):
                        orderByType = OrderByType.Desc;
                        break;
                    default:
                        orderByType = OrderByType.Asc;
                        break;
                }
                var orderByExpression = new OrderByExpression(orderByType, columnExpression);
                var result = new SelectExpression(null, "", table.Columns, table, orderBy: new List<OrderByExpression>() { orderByExpression });
                return result;
            }
        }
        else if (sourceExpression is SelectExpression selectExpression)
        {
            selectExpression = NestSelectExpression(selectExpression);

            if (bodyExpression is ColumnExpression columnExpression)
            {
                OrderByType orderByType;
                switch (whereCall.Method.Name)
                {
                    case nameof(Queryable.ThenBy):
                    case nameof(Queryable.OrderBy):
                        orderByType = OrderByType.Asc;
                        break;
                    case nameof(Queryable.OrderByDescending):
                    case nameof(Queryable.ThenByDescending):
                        orderByType = OrderByType.Desc;
                        break;
                    default:
                        orderByType = OrderByType.Asc;
                        break;
                }
                var orderByExpression = new OrderByExpression(orderByType, columnExpression);
                selectExpression.OrderBy.Add(orderByExpression);

                return selectExpression;
            }
        }
        else
        {
            throw new NotSupportedException(nameof(bodyExpression));
        }

        return whereCall;
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
        if (sourceExpression is TableExpression table)
        {
            if (bodyExpression is ColumnExpression columnExpression)
            {
                var groupByExpression = new GroupByExpression(columnExpression);
                _lastGroupByExpressions = new List<GroupByExpression>() { groupByExpression };
                var result = new SelectExpression(null, "", table.Columns, table, groupBy: new List<GroupByExpression>() { groupByExpression });
                return result;
            }
            else if (bodyExpression is GroupBysExpression groupBysExpression)
            {
                _lastGroupByExpressions = groupBysExpression.GroupByExpressions;
                var result = new SelectExpression(null, "", table.Columns, table, groupBy: groupBysExpression.GroupByExpressions);
                return result;
            }
            else
            {
                throw new NotSupportedException(nameof(bodyExpression));
            }
        }
        else if (sourceExpression is SelectExpression selectExpression)
        {
            selectExpression = NestSelectExpression(selectExpression);

            if (bodyExpression is ColumnExpression columnExpression)
            {
                var groupByExpression = new GroupByExpression(columnExpression);
                _lastGroupByExpressions = new List<GroupByExpression>() { groupByExpression };
                if (selectExpression.GroupBy.IsNotNullAndNotEmpty())
                {
                    selectExpression.GroupBy.Add(groupByExpression);
                }

                return selectExpression;
            }
            else if (bodyExpression is GroupBysExpression groupBysExpression)
            {
                if (selectExpression.GroupBy.IsNotNullAndNotEmpty())
                {
                    selectExpression.GroupBy.AddRange(groupBysExpression.GroupByExpressions);
                }
                _lastGroupByExpressions = groupBysExpression.GroupByExpressions;
                return selectExpression;
            }
            else
            {
                throw new NotSupportedException(nameof(bodyExpression));
            }
        }
        else
        {
            throw new NotSupportedException(nameof(bodyExpression));
        }

        return groupByCall;
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
        if (middleResult is ColumnExpression columnExpression && columnExpression.Type == typeof(bool))
        {
            var whereConditionExpression = new WhereConditionExpression(columnExpression, "=", 1);
            whereConditionExpression.ValueType = typeof(int);
            var result = new FunctionWhereConditionExpression(operatorString, whereConditionExpression);
            return result;
        }
        else if (middleResult is WhereConditionExpression whereConditionExpression)
        {
            var result = new FunctionWhereConditionExpression(operatorString, whereConditionExpression);
            return result;
        }
        else if (middleResult is FunctionWhereConditionExpression functionWhereConditionExpression)
        {
            var result = new FunctionWhereConditionExpression(operatorString, functionWhereConditionExpression);
            return result;
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
            var alias = GetNewAlias();
            tableNameAliasMapping.TryAdd(tableName, alias);
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
                    Value = alias
                }),
            };

            result.Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>(),
                From = table
            };
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
                return new WrapperExpression() { SqlExpression = sqlSelectItemExpression };
            }
        }
        else if (IsNumericType(constant.Value))
        {
            return new WrapperExpression() { SqlExpression = new SqlNumberExpression()
            {
                Value = (decimal)constant.Value
            } };
        }
        return base.VisitConstant(constant);
    }

    public bool IsNumericType(object obj)
    {
        return obj is byte || obj is sbyte ||
               obj is short || obj is ushort ||
               obj is int || obj is uint ||
               obj is long || obj is ulong ||
               obj is float || obj is double ||
               obj is decimal;
    }



    protected override Expression VisitParameter(ParameterExpression param)
    {
        var tableName = DbQueryUtil.GetTableName(param.Type);
        var tableExpression = AppendQualifier(new SqlIdentifierExpression()
        {
            Value = tableName
        });

        return new WrapperExpression()
        {
            SqlExpression = tableExpression
        };
    }

    private object GetValue(Expression member)
    {
        var objectMember = Expression.Convert(member, typeof(object));
        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
        var getter = getterLambda.Compile();
        return getter();
    }

    protected override Expression VisitMember(MemberExpression memberExpression)
    {
        //区分groupBy,单独提取列名
        if (LastMethodName == nameof(Queryable.GroupBy) && memberExpression.Expression is ParameterExpression groupByParameterExpression && groupByParameterExpression.Type.IsGenericType && groupByParameterExpression.Type.GetGenericTypeDefinition().FullName == "System.Linq.IGrouping`2")
        {
            if (memberExpression.Member.Name == "Key")
            {
                var columnsExpression =
                    new ColumnsExpression(_lastGroupByExpressions.Select(it => it.ColumnExpression).ToList());

                return columnsExpression;
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
                return new WrapperExpression()
                {
                    SqlExpression = sqlSelectItemExpression
                };
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
                return new WrapperExpression()
                {
                    SqlExpression = sqlSelectItemExpression
                };
            }
        }
        else if (MethodName == nameof(RepositoryMethod.JoinOn) || MethodName == nameof(RepositoryMethod.MultiQueryWhere) || MethodName == nameof(RepositoryMethod.MultiQueryOrderBy) || MethodName == nameof(RepositoryMethod.MultiSelect))
        {
            //解析静态值,例如dto里的参数，dto.Name
            if (GetNumberOfMemberExpressionLayers(memberExpression) > 0 && GetMemberExpressionLastExpression(memberExpression) is ConstantExpression constantExpression)
            {
                var value = GetValue(memberExpression);
                return Expression.Constant(value);
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
                if (memberExpression.Member is PropertyInfo propertyInfo && propertyInfo.GetMethod?.Name == "get_Length")
                {
                    //获取所有列
                    var middlExpression = this.Visit(parentExpression);
                    if (middlExpression is ColumnExpression column)
                    {
                        column.FunctionName = "LEN";
                        return column;
                    }
                    else
                    {
                        throw new NotSupportedException(nameof(middlExpression));
                    }
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
                        var value = GetValue(memberExpression);
                        return Expression.Constant(value);
                    }

                }

            }
            //如果是it.name这种形式
            else if (memberExpression.Expression is ParameterExpression parameterExpression)
            {
                //获取所有列
                this.VisitParameter(parameterExpression);
                //找到要获取的那一列
                var column = _lastColumns.Values.FirstOrDefault(it => it.MemberInfo.Name == memberExpression.Member.Name);
                if (column == null)
                {
                    throw new NotSupportedException(memberExpression.Member.Name);
                }
                return column;

            }
            //如果是constant
            else if (memberExpression.Expression is ConstantExpression constantExpression)
            {
                var value = GetValue(memberExpression);
                return Expression.Constant(value);
                //return constantExpression;
            }
            //如果是匿名类
            else if (memberExpression.Expression is NewExpression newExpression && newExpression.Arguments.Any() && newExpression.Arguments[0] is ConstantExpression)
            {
                var value = GetValue(memberExpression);
                return Expression.Constant(value);
                //return constantExpression;
            }
            //如果是new一个实例，如new TestWhereNewDatetime().Time
            else if (memberExpression.Expression is NewExpression newExpression2)
            {
                var value = GetValue(memberExpression);
                return Expression.Constant(value);
                //return constantExpression;
            }
            else
            {
                var result = new ColumnExpression(null, "", null, 0);
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
        if (newExpression.Members == null)
        {
            var value = GetValue(newExpression);
            return Expression.Constant(value);
        }

        for (int i = 0; i < newExpression.Members.Count; i++)
        {
            var memberInfo = newExpression.Members[i];
            var argument = newExpression.Arguments[i];
            var middleResult = this.Visit(argument);
            if (middleResult is WrapperExpression { SqlExpression: SqlSelectItemExpression sqlSelectItemExpression } wrapperExpression)
            {
                sqlSelectItemExpression.Alias = new SqlIdentifierExpression()
                {
                    Value = DbQueryUtil.GetColumnName(memberInfo)
                };
                AppendSelectItemToResult(sqlSelectItemExpression);
            }
            else
            {
                throw new NotSupportedException("parse error");
            }
        }

        return newExpression;

    }

}