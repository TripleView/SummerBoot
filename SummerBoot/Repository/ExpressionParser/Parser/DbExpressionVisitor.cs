using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using SummerBoot.Core;
using SummerBoot.Repository.ExpressionParser.Util;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class DbExpressionVisitor : ExpressionVisitor
    {
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
        /// <summary>
        /// 上一次处理的方法名称
        /// </summary>
        private string LastMethodName => lastMethodCalls.LastOrDefault();
        #region 表名生成管理

        private int _tableIndex = -1;

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
                case DbExpressionType.Table:
                    return this.VisitTable((TableExpression)exp);
                case DbExpressionType.Column:
                    return this.VisitColumn((ColumnExpression)exp);
            }

            return base.Visit(exp);
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
                    //MethodName = method.Name;
                    var result = this.VisitSelectCall(node);
                    var lastMethodCallName = methodCallStack.Pop();
                    lastMethodCalls.Add(lastMethodCallName);
                    return result;
               
                case nameof(Queryable.Where):
                
                    methodCallStack.Push(nameof(Queryable.Where));
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
                case   nameof(Queryable.Max):
                case   nameof(Queryable.Min):
                    methodCallStack.Push(methodName);
                    var result8 = this.VisitMaxMinCall(node);
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
                            throw new NotSupportedException(methodName);
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
                    else
                    {
                        throw new NotSupportedException(methodName);
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
            else if (MethodName == nameof(Queryable.Where)||MethodName==nameof(Queryable.FirstOrDefault) || MethodName == nameof(Queryable.First) || MethodName == nameof(Queryable.Count))
            {
                var @operator = nodeTypeMappings[binaryExpression.NodeType];
                if (string.IsNullOrWhiteSpace(@operator))
                {
                    throw new NotSupportedException(nameof(binaryExpression.NodeType));
                }
                var result = new WhereExpression(@operator);
                var rightExpression = this.Visit(binaryExpression.Right);

                var leftExpression = this.Visit(binaryExpression.Left);

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
                }
                else if (rightExpression is ColumnExpression rightColumnExpression2 && rightExpression.Type == typeof(bool) && leftExpression is WhereExpression leftWhereExpression2)
                {
                    //如果是column类型的bool值，默认为true
                    var right = new WhereConditionExpression(rightColumnExpression2, "=", 1) { ValueType = rightColumnExpression2.ValueType };
                    result.Left = leftWhereExpression2;
                    result.Right = right;
                }
                else if (rightExpression is ColumnExpression rightColumnExpression3 && rightExpression.Type == typeof(bool) && leftExpression is ColumnExpression leftColumnExpression3 && leftColumnExpression3.Type == typeof(bool))
                {
                    //如果是column类型的bool值，默认为true
                    var right = new WhereConditionExpression(rightColumnExpression3, "=", 1) { ValueType = rightColumnExpression3.ValueType };
                    var left = new WhereConditionExpression(leftColumnExpression3, "=", 1) { ValueType = leftColumnExpression3.ValueType };
                    result.Left = left;
                    result.Right = right;
                }
                else if (leftExpression is WhereExpression leftWhereExpression &&
                   rightExpression is WhereExpression rightWhereExpression)
                {
                    result.Left = leftWhereExpression;
                    result.Right = rightWhereExpression;

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
                }
                else
                {
                    throw new NotSupportedException(MethodName);
                }
                return result;

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
                }else if (methodName == nameof(Queryable.Count))
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
                    result.Where=where;
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

        public virtual Expression VisitMaxMinCall(MethodCallExpression maxMinCall)
        {
            var methodName = maxMinCall.Method.Name;
            var sourceExpression = this.Visit(maxMinCall.Arguments[0]);
            ColumnExpression column = null;
            if (maxMinCall.Arguments.Count == 2)
            {
                var lambda = (LambdaExpression)this.StripQuotes(maxMinCall.Arguments[1]);
                column = this.Visit(lambda.Body) as ColumnExpression;
            }

            if (sourceExpression is TableExpression table)
            {
                var result = new SelectExpression(null, "", table.Columns, table);

                result.Columns.Clear();
                column.FunctionName = methodName;
                result.Columns.Add(column);

                return result;
            }
            else if (sourceExpression is SelectExpression selectExpression)
            {
                return selectExpression;
            }
            else
            {
                throw new NotSupportedException(nameof(maxMinCall));
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
                var source = (TableExpression)this.Visit(whereCall.Arguments[0]);
                var lambda = (LambdaExpression)this.StripQuotes(whereCall.Arguments[1]);
                var bodyExpression = this.Visit(lambda.Body);


                //兼容it.isHandsome这种单true的条件
                if (bodyExpression is ColumnExpression columnExpression && columnExpression.Type == typeof(bool))
                {
                    var whereConditionExpression = new WhereConditionExpression(columnExpression, "=", 1);
                    var result = new SelectExpression(null, "", source.Columns, source, whereConditionExpression);
                    return result;
                }
                else if (bodyExpression is WhereExpression whereExpression)
                {
                    var result = new SelectExpression(null, "", source.Columns, source, whereExpression);
                    return result;
                }
                //兼容It=>true这种
                else if (bodyExpression is ConstantExpression constantExpression && constantExpression.Type == typeof(bool))
                {
                    var value = (bool)constantExpression.Value;
                    var whereTrueFalseValueCondition = new WhereTrueFalseValueConditionExpression(value);
                    var result = new SelectExpression(null, "", source.Columns, source, whereTrueFalseValueCondition);
                    return result;
                }
                else
                {
                    throw new NotSupportedException(nameof(bodyExpression));
                }

                return whereCall;
            }

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
            var sourceExpression = this.Visit(selectCall.Arguments[0]);
            var lambda = (LambdaExpression)this.StripQuotes(selectCall.Arguments[1]);
            var bodyExpression = this.Visit(lambda.Body);
            if (sourceExpression is TableExpression table)
            {
                if (bodyExpression is ColumnExpression columnExpression)
                {

                    var result = new SelectExpression(null, "", new List<ColumnExpression>() { columnExpression }, table);

                    return result;
                }
                else if (bodyExpression is ColumnsExpression columnsExpression)
                {

                    var result = new SelectExpression(null, "", columnsExpression.ColumnExpressions, table);

                    return result;
                }
            }
            else if (sourceExpression is SelectExpression selectExpression)
            {
                selectExpression = NestSelectExpression(selectExpression);

                if (bodyExpression is ColumnExpression columnExpression)
                {
                    selectExpression.Columns = new List<ColumnExpression>() { columnExpression };
                }
                else if (bodyExpression is ColumnsExpression columnsExpression)
                {
                    selectExpression.Columns = columnsExpression.ColumnExpressions;
                }
                return selectExpression;
            }

            return selectCall;
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
                //生成TableExpression,并将其Columns属性缓存

                var tableExpression = new TableExpression(queryable.ElementType);

                _lastColumns = tableExpression.Columns.ToDictionary(x => x.ColumnName);

                return tableExpression;
            }

            return base.VisitConstant(constant);
        }

        protected override Expression VisitParameter(ParameterExpression param)
        {

            //如果缓存中没有任何列
            if (_lastColumns.Count == 0) return base.VisitParameter(param);

            //根据_lastColumns中生成newColumns,Name = Expression.Constant(oldColumn)也就是对oldColumn的一个引用
            var newColumns = _lastColumns.Values.Select(oldColumn =>
                new ColumnExpression(oldColumn.Type,
                    oldColumn.TableAlias,
                    oldColumn.MemberInfo,
                    oldColumn.Index,oldColumn.ValueType)).ToList();

            ////将生成的新列赋值给缓存
            _lastColumns = newColumns.ToDictionary(it => it.ColumnName);

            return new ColumnsExpression(newColumns);

            if (MethodName == nameof(Queryable.Select) || MethodName == nameof(Queryable.Where) || MethodName == nameof(Queryable.GroupBy) || MethodName == nameof(Queryable.OrderBy))
            {

            }

            throw new NotSupportedException(nameof(param));


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
                    return Expression.Constant(value);
                }
                else
                {

                    //如果缓存中没有任何列
                    if (_lastColumns.Count == 0) return base.VisitMember(memberExpression);
                    var propertyInfo = memberExpression.Member;
                    var columnName = DbQueryUtil.GetColumnName(propertyInfo);

                    var alias = this.GetNewAlias();
                    var columnExpression = new ColumnExpression(propertyInfo.DeclaringType, alias, propertyInfo, 0);

                    return columnExpression;
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
                        var value = GetValue(memberExpression);
                        return Expression.Constant(value);
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
                else
                {
                    var result = new ColumnExpression(null, "", null, 0);
                }
            }

            throw new NotSupportedException(this.MethodName);

        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            var newColumns = new List<ColumnExpression>();

            for (int i = 0; i < newExpression.Members.Count; i++)
            {
                var memberInfo = newExpression.Members[i];
                var argument = newExpression.Arguments[i];
                if (argument is MemberExpression memberExpression)
                {
                    var middleResult = this.Visit(memberExpression);
                    if (middleResult == null)
                    {
                        continue;
                    }
                    else if (middleResult is ConstantExpression constantExpression)
                    {
                        var value = constantExpression.Value;
                        var newColumn = new ColumnExpression(constantExpression.Type, "", memberInfo, i, value);
                        newColumns.Add(newColumn);
                    }
                    else if (middleResult is ColumnExpression columnExpression)
                    {
                        newColumns.Add(columnExpression);
                    }
                    //针对groupby的select进行特殊处理,类似于g.key这种，这个key可能是单个字段，也可能是多个字段
                    else if (LastMethodName == nameof(Queryable.GroupBy) && middleResult is ColumnsExpression columnsExpression)
                    {
                        var columnCount = columnsExpression.ColumnExpressions.Count;

                        foreach (var column in columnsExpression.ColumnExpressions)
                        {
                            var tempColumnExpression = new ColumnExpression(column.Type, column.TableAlias,
                                column.MemberInfo, 0, columnCount > 1 ? "" : memberInfo.Name, column.FunctionName);
                            newColumns.Add(tempColumnExpression);
                        }

                    }
                }
                else if (argument is ConstantExpression constantExpression)
                {
                    var value = constantExpression.Value;
                    var newColumn = new ColumnExpression(constantExpression.Type, "", memberInfo, i, value);
                    newColumns.Add(newColumn);
                }
                else if (argument is MethodCallExpression methodCallExpression)
                {
                    var value = this.Visit(argument);
                    if (value is ColumnExpression columnExpression)
                    {
                        //针对groupby的select进行特殊处理
                        if (LastMethodName == nameof(Queryable.GroupBy))
                        {
                            columnExpression = new ColumnExpression(columnExpression.Type, columnExpression.TableAlias,
                                columnExpression.MemberInfo, 0, memberInfo.Name, columnExpression.FunctionName);
                        }
                        newColumns.Add(columnExpression);
                    }
                    else
                    {
                        throw new NotSupportedException(nameof(newExpression));
                    }
                }
                //else if (argument is MethodCallExpression methodCallExpression)
                //{
                //    var value = constantExpression.Name;
                //    var newColumn = new ColumnExpression(constantExpression.Type, "", memberInfo, i, value);
                //    newColumns.Add(newColumn);
                //}

            }

            switch (MethodName)
            {
                case nameof(Queryable.Select):
                case nameof(Queryable.Where):
                    var result = new ColumnsExpression(newColumns);
                    return result;
                    break;
                case nameof(Queryable.GroupBy):

                    var groupByExpressions = new List<GroupByExpression>();
                    foreach (var columnExpression in newColumns)
                    {
                        var groupByExpression = new GroupByExpression(columnExpression);
                        groupByExpressions.Add(groupByExpression);
                    }

                    var result2 = new GroupBysExpression(groupByExpressions);
                    return result2;
                    break;
            }


            throw new NotSupportedException(nameof(newExpression));
        }

    }
}