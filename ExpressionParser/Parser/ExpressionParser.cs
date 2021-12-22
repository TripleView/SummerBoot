using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ExpressionParser.Base;

namespace ExpressionParser.Parser
{
    public class ExpressionParser<T> : IExpressionParser<T> where T : class
    {
        private SqlInfo sqlInfo;
        private TableInfo<T> tableInfo;
        private ParserOption parserOption;

        /// <summary>
        /// 判断是否正在处理bool类型，因为bool类型属性本身默认为true，需要特殊处理
        /// </summary>
        private bool isHandleBool = false;

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

        public ExpressionParser(DatabaseType databaseType)
        {
            sqlInfo = new SqlInfo(databaseType);
            tableInfo = new TableInfo<T>();
            parserOption = new ParserOption();
        }

        public ExpressionParser(DatabaseType databaseType, ParserOption parserOption)
        {
            sqlInfo = new SqlInfo(databaseType);
            tableInfo = new TableInfo<T>();
            parserOption = parserOption ?? new ParserOption();
        }

        public IExpressionParser<T> Select(Expression<Func<T, object>> expression)
        {
            sqlInfo.SetSqlOperationPartType(SqlOperationPartType.Select);

            HandleExpressionByRecursion(expression);

            return this;
        }

        public string Select2<T1, T2>(Expression<Func<T1, T2, object>> expression)
        {
            sqlInfo.SetSqlOperationPartType(SqlOperationPartType.Select);

            HandleExpressionByRecursion(expression);

            return "";
        }
        public IExpressionParser<T> Where(Expression<Func<T, bool>> expression)
        {
            sqlInfo.SetSqlOperationPartType(SqlOperationPartType.Where);

            HandleExpressionByRecursion(expression);

            return this;
        }

        public void HandleExpressionByRecursion(Expression expression)
        {
            switch (expression)
            {
                case ParameterExpression parameterExpression:
                    break;
                case UnaryExpression unaryExpression:
                    UnaryExpressionParse(unaryExpression);
                    break;
                case LambdaExpression lambdaExpression:
                    LambdaExpressionParse(lambdaExpression);
                    break;
                case ConstantExpression constantExpression:
                    ConstantExpressionParse(constantExpression);
                    break;
                case MemberExpression memberExpression:
                    MemberExpressionParse(memberExpression);
                    break;
                case BinaryExpression binaryExpression:
                    BinaryExpressionParse(binaryExpression);
                    break;
                case MethodCallExpression methodCallExpression:
                    MethodCallExpressionParse(methodCallExpression);
                    break;

            }
        }

        public void MethodCallExpressionParse(MethodCallExpression methodCallExpression)
        {
            var methodName = methodCallExpression.Method.Name;
            var methodCallObject = methodCallExpression.Object;
            //判断是常数调用方法，还是变量调用方法


            if (methodCallExpression.Method.DeclaringType == typeof(string))
            {
                var conventionMethodNameList = new List<string>() { "Trim", "TrimEnd", "TrimStart", "ToUpper", "ToLower" };
                if (conventionMethodNameList.Contains(methodName))
                {
                    //转换后的方法名称
                    var afterConventionMethodName = "";
                    switch (methodCallExpression.Method.Name)
                    {
                        case "Trim":
                            afterConventionMethodName = "TRIM";
                            break;
                        case "TrimEnd":
                            afterConventionMethodName = "RTRIM";

                            break;
                        case "TrimStart":

                            afterConventionMethodName = "LTRIM";
                            break;
                        case "ToUpper":
                            afterConventionMethodName = "UPPER";
                            break;
                        case "ToLower":
                            afterConventionMethodName = "*";
                            break;
                    }

                    sqlInfo.AddSql(afterConventionMethodName + "(");
                    HandleExpressionByRecursion(methodCallObject);
                    sqlInfo.AddSql(")");
                }
                var likeMethodNameList = new List<string>() { "Contains", "StartsWith", "EndsWith" };
                if (likeMethodNameList.Contains(methodName))
                {
                    var value = "";
                    var argOne = methodCallExpression.Arguments[0];
                    switch (methodName)
                    {
                        case "Contains":
                            value = "%" + GetValue(argOne).ToString() + "%";
                            break;
                        case "StartsWith":
                            value = "" + GetValue(argOne).ToString() + "%";
                            break;
                        case "EndsWith":
                            value = "%" + GetValue(argOne).ToString();
                            break;
                    }
                    HandleExpressionByRecursion(methodCallObject);
                    sqlInfo.AddSql("like ");
                    sqlInfo.AddParameter(value);
                }

                if (methodName == "Equals")
                {
                    var argOne = methodCallExpression.Arguments[0];
                    HandleExpressionByRecursion(methodCallObject);
                    sqlInfo.AddSql("= ");
                    HandleExpressionByRecursion(argOne);
                }
            }

            //throw new Exception("Unsupported method call: " + expression.Method.Name);
        }

        public void BinaryExpressionParse(BinaryExpression binaryExpression)
        {
            sqlInfo.AddSql("(");
            var leftPart = binaryExpression.Left;
            CheckIsBoolType(leftPart, binaryExpression);
            HandleExpressionByRecursion(leftPart);

            var operatorString = nodeTypeMappings[binaryExpression.NodeType];
            sqlInfo.AddSql(operatorString);

            var rightPart = binaryExpression.Right;
            CheckIsBoolType(rightPart, binaryExpression);
            HandleExpressionByRecursion(rightPart);
            sqlInfo.AddSql(")");
        }
        public void UnaryExpressionParse(UnaryExpression unaryExpression)
        {
            var operatorString = nodeTypeMappings[unaryExpression.NodeType];
            sqlInfo.AddSql(operatorString + "(");
            var part = unaryExpression.Operand;
            CheckIsBoolType(part, unaryExpression);
            HandleExpressionByRecursion(part);
            sqlInfo.AddSql(")");
        }

        public void MemberExpressionParse(MemberExpression memberExpression)
        {
            var memberInfo = memberExpression.Member;

            switch (memberExpression.Expression)
            {
                case ParameterExpression parameterExpression:
                    var sql = tableInfo.GetSqlSnippetByPropertyName(memberInfo.Name);

                    sqlInfo.AddSql(sql);

                    ProcessBoolType();
                    break;
                case MemberExpression newMemberExpression:
                    var result = GetValue(memberExpression);
                    sqlInfo.AddParameter(result);
                    break;
            }
           
           
        }

        /// <summary>
        /// 统一处理布尔类型
        /// </summary>
        private void ProcessBoolType()
        {
            if (isHandleBool)
            {
                //var valueToString = value ? parserOption.TrueMapNum : parserOption.FalseMapNum;
                sqlInfo.AddSql("=" + parserOption.TrueMapNum);
                isHandleBool = false;
            }
        }
        public void LambdaExpressionParse(LambdaExpression expression)
        {
            var body = expression.Body;
            CheckIsBoolType(body, expression);
            HandleExpressionByRecursion(body);
        }

        /// <summary>
        /// 判断是否正在处理bool类型，因为bool类型属性本身默认为true，需要特殊处理
        /// </summary>
        /// <param name="body"></param>
        /// <param name="parentExpression"></param>
        private void CheckIsBoolType(Expression body, Expression parentExpression)
        {
            switch (parentExpression)
            {
                case BinaryExpression binaryExpression:
                    //针对bool类型进行特殊处理
                    var anotherBody = binaryExpression.Left.Equals(body) ? binaryExpression.Right : binaryExpression.Left;

                    //if ((body.Type == typeof(bool) && (body.NodeType == ExpressionType.MemberAccess || body.NodeType == ExpressionType.Constant))
                    //    && anotherBody.NodeType != ExpressionType.Constant)
                    //判断二元表达式是否为=或者<>类型
                    var binaryExpressionNodeTypeIsEqualOrNotEqual =
                        binaryExpression.NodeType == ExpressionType.Equal ||
                        binaryExpression.NodeType == ExpressionType.NotEqual;

                    if (body.Type == typeof(bool) && body.NodeType == ExpressionType.MemberAccess
                    && !binaryExpressionNodeTypeIsEqualOrNotEqual)
                    {
                        isHandleBool = true;
                    }
                    if (body.Type == typeof(bool) && body.NodeType == ExpressionType.Constant && !binaryExpressionNodeTypeIsEqualOrNotEqual)
                    {
                        isHandleBool = true;
                    }
                    break;
                case LambdaExpression lambdaExpression:
                case UnaryExpression unaryExpression:
                    if ((body is MemberExpression expression || body is ConstantExpression constantExpression) && body.Type == typeof(bool))
                    {
                        isHandleBool = true;
                    }

                    break;
            }


        }
        public void ConstantExpressionParse(ConstantExpression constantExpression)
        {
            var value = constantExpression.Value;
            if (constantExpression.Type == typeof(bool))
            {
                var boolValue = (bool)value;
                if (boolValue)
                {
                    sqlInfo.AddSql(parserOption.TrueMapNum.ToString());
                }
                else
                {
                    sqlInfo.AddSql(parserOption.FalseMapNum.ToString());
                }

                ProcessBoolType();
            }
            else
            {
                sqlInfo.AddParameter(value);
            }

        }

        private object GetValue(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }

        public string GenerateSql()
        {
            var result = sqlInfo.GetSql();
            sqlInfo = new SqlInfo(this.sqlInfo.DatabaseType);
            return result;
        }
    }
}