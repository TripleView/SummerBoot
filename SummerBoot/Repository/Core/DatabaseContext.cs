using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using SummerBoot.Core;
using SummerBoot.Core.Utils.Reflection;
using YamlDotNet.Core.Tokens;

namespace SummerBoot.Repository.Core
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public static class DatabaseContext
    {
        private static ConcurrentDictionary<string, object> ChangeTypeToOtherTypeCache = new ConcurrentDictionary<string, object>();
        /// <summary>
        /// 查询的时候用的behavior标志
        /// </summary>
        private static CommandBehavior queryBehavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult;

        private static CommandBehavior queryFirstOrDefaultBehavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        private static CommandBehavior queryMultipleBehavior = CommandBehavior.SequentialAccess;


        private static Task TryOpenAsync(this IDbConnection cnn, CancellationToken cancel)
        {
            if (cnn is DbConnection dbConn)
            {
                return dbConn.OpenAsync(cancel);
            }
            else
            {
                throw new InvalidOperationException("Async operations require use of a DbConnection or an already-open IDbConnection");
            }
        }

        private static async Task<int> ExecuteNonQueryAsync(this IDbCommand cmd, CancellationToken cancel)
        {
            if (cmd is DbCommand dbCommand)
            {
                var result = await dbCommand.ExecuteNonQueryAsync(cancel);
                return result;
            }
            else
            {
                throw new InvalidOperationException("Async operations require use of a DbCommand");
            }
        }

        private static async Task<DbDataReader> ExecuteReaderWithCommandBehaviorAsync(this IDbCommand cmd, CommandBehavior behavior, CancellationToken cancel)
        {
            if (cmd is DbCommand dbCommand)
            {
                var result = await dbCommand.ExecuteReaderAsync(behavior, cancel);
                return result;
            }
            else
            {
                throw new InvalidOperationException("Async operations require use of a DbCommand");
            }
        }


        public static async Task<T> QueryFirstOrDefaultAsync<T>(this IDbConnection dbConnection, DatabaseUnit databaseUnit, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken token = default)
        {
            if (sql.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("sql cannot be empty");
            }

            var effectiveType = typeof(T);
            bool wasClosed = dbConnection.State == ConnectionState.Closed;

            DbDataReader reader = null;
            IDbCommand cmd = null;

            T result = default;
            try
            {
                if (wasClosed)
                {
                    await dbConnection.TryOpenAsync(token).ConfigureAwait(false);
                }

                cmd = SetUpDbCommand(dbConnection, databaseUnit, sql, param, transaction, commandTimeout, commandType, token);



                reader = await cmd.ExecuteReaderWithCommandBehaviorAsync(GetBehavior(wasClosed, queryFirstOrDefaultBehavior), token);

                var func = GetDeserializer(typeof(T), reader, databaseUnit);

                if (await reader.ReadAsync(token).ConfigureAwait(false) && reader.FieldCount != 0)
                {
                    object val = func(reader);
                    result = GetValue<T>(reader, effectiveType, val);
                }
                reader?.Dispose();
                reader = null;
                return result;
            }
            finally
            {
                reader?.Dispose();
            }
        }

        private static IDbCommand SetUpDbCommand(IDbConnection dbConnection, DatabaseUnit databaseUnit, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken token = default)
        {
            var cmd = dbConnection.CreateCommand();
            cmd.Init();

            cmd.CommandType = commandType ?? CommandType.Text;
            cmd.CommandTimeout = commandTimeout ?? databaseUnit.CommandTimeout;

            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }

            //设置参数
            if (param != null)
            {
                var dbParameters = new DynamicParameters(param);
                cmd.SetUpParameter(dbParameters, databaseUnit, ref sql);
            }

            cmd.CommandText = sql;

            return cmd;
        }

        public static async Task<IEnumerable<T>> QueryAsync<T>(this IDbConnection dbConnection, DatabaseUnit databaseUnit, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken token = default)
        {
            if (sql.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("sql can not be empty");
            }

            var effectiveType = typeof(T);
            bool wasClosed = dbConnection.State == ConnectionState.Closed;

            IDataReader reader = null;
            IDbCommand cmd = null;
            try
            {
                if (wasClosed)
                {
                    await dbConnection.TryOpenAsync(token).ConfigureAwait(false);
                }

                cmd = SetUpDbCommand(dbConnection, databaseUnit, sql, param, transaction, commandTimeout, commandType, token);


                reader = await cmd.ExecuteReaderWithCommandBehaviorAsync(GetBehavior(wasClosed, queryBehavior), token);

                var func = GetDeserializer(typeof(T), reader, databaseUnit);

                var result = ExecuteReaderSync<T>(reader, func);
                reader = null;
                return result;
            }
            finally
            {
                reader?.Dispose();
            }
        }

        private static void TryClose(IDbConnection dbConnection)
        {
            if (dbConnection.State == ConnectionState.Open) dbConnection.Close();
        }

        private static async Task TryCloseAsync(IDbConnection dbConnection)
        {

            if (dbConnection is DbConnection tempDbConnection && tempDbConnection.State == ConnectionState.Open)
            {
                await tempDbConnection.CloseAsync();
            }
        }

        private static IEnumerable<T> ExecuteReaderSync<T>(IDataReader reader, Func<IDataReader, object> func)
        {
            var effectiveType = typeof(T);
            using (reader)
            {
                while (reader.Read())
                {
                    object val = func(reader);
                    yield return GetValue<T>(reader, effectiveType, val);
                }
            }
        }


        public static async Task<GridReader> QueryMultipleAsync(this IDbConnection dbConnection, DatabaseUnit databaseUnit, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken token = default)
        {
            if (sql.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("sql cannot be empty");
            }

            bool wasClosed = dbConnection.State == ConnectionState.Closed;

            IDataReader reader = null;
            IDbCommand cmd = null;
            try
            {
                if (wasClosed)
                {
                    await dbConnection.TryOpenAsync(token);
                }
                cmd = SetUpDbCommand(dbConnection, databaseUnit, sql, param, transaction, commandTimeout, commandType, token);


                reader = await ExecuteReaderWithCommandBehaviorAsync(cmd, GetBehavior(wasClosed, queryMultipleBehavior), token);

                var result = new GridReader(reader, cmd, databaseUnit);
                cmd = null;
                return result;
            }
            finally
            {

            }
        }

        /// <summary>
        /// 获取执行行为，如果是在事务中，则是不需要自动关闭链接的
        /// 
        /// </summary>
        /// <param name="wasClosed"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        private static CommandBehavior GetBehavior(bool wasClosed, CommandBehavior behavior)
        {
            return wasClosed ? behavior | CommandBehavior.CloseConnection : behavior;
        }

        public static GridReader QueryMultiple(this IDbConnection dbConnection, DatabaseUnit databaseUnit, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (sql.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("sql cannot be empty");
            }

            bool wasClosed = dbConnection.State == ConnectionState.Closed;

            IDataReader reader = null;
            IDbCommand cmd = null;
            try
            {
                if (wasClosed)
                {
                    dbConnection.Open();
                }
                cmd = SetUpDbCommand(dbConnection, databaseUnit, sql, param, transaction, commandTimeout, commandType);


                reader = ExecuteReaderWithCommandBehavior(cmd, GetBehavior(wasClosed, queryMultipleBehavior));

                var result = new GridReader(reader, cmd, databaseUnit);
                cmd = null;
                return result;
            }
            finally
            {

            }
        }


        public static IEnumerable<T> Query<T>(this IDbConnection dbConnection, DatabaseUnit databaseUnit, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (sql.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("sql cannot be empty");
            }

            var effectiveType = typeof(T);
            bool wasClosed = dbConnection.State == ConnectionState.Closed;

            IDataReader reader = null;
            IDbCommand cmd = null;
            try
            {
                if (wasClosed)
                {
                    dbConnection.Open();
                }
                cmd = SetUpDbCommand(dbConnection, databaseUnit, sql, param, transaction, commandTimeout, commandType);

                reader = ExecuteReaderWithCommandBehavior(cmd, GetBehavior(wasClosed, queryBehavior));

                var func = GetDeserializer(typeof(T), reader, databaseUnit);

                while (reader.Read() && reader.FieldCount != 0)
                {
                    object val = func(reader);
                    yield return GetValue<T>(reader, effectiveType, val);
                }

                reader.Dispose();
                reader = null;
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        try
                        {
                            cmd.Cancel();
                        }
                        catch
                        {

                        }
                    }
                    reader.Dispose();
                }

                cmd?.Parameters.Clear();
                cmd?.Dispose();
            }
        }

        public static T QueryFirstOrDefault<T>(this IDbConnection dbConnection, DatabaseUnit databaseUnit, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (sql.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("sql cannot be empty");
            }

            var effectiveType = typeof(T);
            bool wasClosed = dbConnection.State == ConnectionState.Closed;

            IDataReader reader = null;
            IDbCommand cmd = null;
            T result = default;
            try
            {
                if (wasClosed)
                {
                    dbConnection.Open();
                }
                cmd = SetUpDbCommand(dbConnection, databaseUnit, sql, param, transaction, commandTimeout, commandType);


                reader = ExecuteReaderWithCommandBehavior(cmd, GetBehavior(wasClosed, queryFirstOrDefaultBehavior));

                var func = GetDeserializer(typeof(T), reader, databaseUnit);
                if (reader.Read() && reader.FieldCount != 0)
                {
                    object val = func(reader);
                    result = GetValue<T>(reader, effectiveType, val);
                }

                reader.Dispose();
                reader = null;
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        try
                        {
                            cmd.Cancel();
                        }
                        catch
                        {

                        }
                    }
                    reader.Dispose();
                }

                cmd?.Parameters.Clear();
                cmd?.Dispose();
            }

            return result;
        }


        private static IDataReader ExecuteReaderWithCommandBehavior(IDbCommand cmd, CommandBehavior behavior)
        {
            try
            {
                return cmd.ExecuteReader(behavior);
            }
            catch (ArgumentException ex)
            {
                throw;
            }
        }

        public static async Task<int> ExecuteAsync(this IDbConnection dbConnection, DatabaseUnit databaseUnit, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken token = default)
        {
            if (sql.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("sql cannot be empty");
            }

            int result = 0;

            bool wasClosed = dbConnection.State == ConnectionState.Closed;
            IDbCommand cmd = null;
            try
            {
                if (wasClosed)
                {
                    await dbConnection.TryOpenAsync(token).ConfigureAwait(false);
                }

                cmd = SetUpDbCommand(dbConnection, databaseUnit, sql, param, transaction, commandTimeout, commandType, token);


                result = await cmd.ExecuteNonQueryAsync(token);
            }
            finally
            {
                cmd?.Parameters.Clear();
                cmd?.Dispose();
            }

            return result;
        }


        public static int Execute(this IDbConnection dbConnection, DatabaseUnit databaseUnit, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (sql.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("sql cannot be empty");
            }

            int result = 0;

            bool wasClosed = dbConnection.State == ConnectionState.Closed;
            IDbCommand cmd = null;

            try
            {
                if (wasClosed)
                {
                    dbConnection.Open();
                }

                cmd = SetUpDbCommand(dbConnection, databaseUnit, sql, param, transaction, commandTimeout, commandType);


                result = cmd.ExecuteNonQuery();
            }
            finally
            {
                cmd?.Parameters.Clear();
                cmd?.Dispose();
            }

            return result;
        }

        private static void Init(this IDbCommand cmd)
        {
            //初始化command
            var cmdFunc = GetInit(cmd.GetType());
            if (cmdFunc != null)
            {
                cmdFunc(cmd);
            }

        }

        private static MethodInfo GetBasicPropertySetter(Type declaringType, string name, Type expectedType)
        {
            var prop = declaringType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop?.CanWrite == true && prop.PropertyType == expectedType && prop.GetIndexParameters().Length == 0)
            {
                return prop.GetSetMethod();
            }
            return null;
        }

        /// <summary>
        /// //针对oracle数据库，一定要设置bindByName，否则无法正确设置参数
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        private static Action<IDbCommand> GetInit(Type commandType)
        {
            if (commandType == null)
                return null;

            Action<IDbCommand> action;


            var bindByName = GetBasicPropertySetter(commandType, "BindByName", typeof(bool));
            var initialLongFetchSize = GetBasicPropertySetter(commandType, "InitialLONGFetchSize", typeof(int));

            action = null;
            if (bindByName != null || initialLongFetchSize != null)
            {
                var method = new DynamicMethod(commandType.Name + "_init", null, new Type[] { typeof(IDbCommand) });
                var il = method.GetILGenerator();

                if (bindByName != null)
                {
                    // .BindByName = true
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, commandType);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.EmitCall(OpCodes.Callvirt, bindByName, null);
                }
                if (initialLongFetchSize != null)
                {
                    // .InitialLONGFetchSize = -1
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, commandType);
                    il.Emit(OpCodes.Ldc_I4_M1);
                    il.EmitCall(OpCodes.Callvirt, initialLongFetchSize, null);
                }
                il.Emit(OpCodes.Ret);
                action = (Action<IDbCommand>)method.CreateDelegate(typeof(Action<IDbCommand>));
            }

            return action;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValue<T>(IDataReader reader, Type type, object val)
        {
            if (val is T tVal)
            {
                return tVal;
            }
            else if (val == null)
            {
                return default;
            }
            else
            {
                try
                {
                    var fromType = val.GetType();
                    Func<object, T> func;
                    var cacheKey = type.FullName + ":" + fromType.FullName;
                    if (ChangeTypeToOtherTypeCache.ContainsKey(cacheKey))
                    {
                        func = (Func<object, T>)ChangeTypeToOtherTypeCache[cacheKey];
                    }
                    else
                    {
                        func = ChangeTypeToOtherType<T>(fromType);
                        ChangeTypeToOtherTypeCache.TryAdd(cacheKey, func);
                    }

                    return func(val);
                }
                catch (Exception ex)
                {
                    ThrowRepositoryException(ex, val, 0, reader);
                    return default;
                }
            }
        }

        public static Func<object, T> ChangeTypeToOtherType<T>(Type fromType)
        {
            var toType = typeof(T);
            var dynamicMethod = new DynamicMethod("ChangeTypeToOtherType" + Guid.NewGuid().ToString("N"), toType,
                new Type[] { typeof(object) });

            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.ConvertTypeToTargetType(fromType, toType);
            il.Emit(OpCodes.Ret);

            var result = (Func<object, T>)dynamicMethod.CreateDelegate(typeof(Func<object, T>));
            return result;
        }

        public static Func<object, object> ChangeTypeToOtherType(Type fromType, Type toType)
        {
            var dynamicMethod = new DynamicMethod("ChangeTypeToOtherType" + Guid.NewGuid().ToString("N"), toType,
                new Type[] { typeof(object) });

            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.ConvertTypeToTargetType(fromType, toType);
            il.Emit(OpCodes.Ret);

            var result = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
            return result;
        }

        /// <summary>
        /// set up parameter
        /// 设置参数
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="dynamicParameters"></param>
        /// <param name="databaseUnit"></param>
        /// <param name="sql"></param>
        public static void SetUpParameter(this IDbCommand dbCommand, DynamicParameters dynamicParameters, DatabaseUnit databaseUnit, ref string sql)
        {
            var parameters = dynamicParameters.GetParamInfos;
            foreach (var info in parameters)
            {
                var paramInfo = info.Value;
                //判断参数是否为列表
                if (paramInfo.ValueType != null && paramInfo.ValueType.IsEnumerable() && !paramInfo.ValueType.IsString())
                {
                    var tempValues = paramInfo.Value as IEnumerable;
                    if (tempValues == null)
                    {
                        var tempParamInfo = new ParamInfo()
                        {
                            Name = info.Key,
                            Value = DBNull.Value,
                            ValueType = typeof(DBNull)
                        };
                        SetUpSingleParameter(dbCommand, info.Key, tempParamInfo, databaseUnit);
                        continue;
                    }

                    var count = 0;
                    foreach (var tempValue in tempValues)
                    {
                        count++;
                        if (count == 1 && tempValue == null)
                        {
                            throw new NotSupportedException("The first item in a list-expansion cannot be null");
                        }
                        var tempParamInfo = new ParamInfo()
                        {
                            Name = info.Key + count,
                            Value = tempValue,
                            ValueType = tempValue.GetType(),
                            ParameterDirection = ParameterDirection.Input
                        };
                        SetUpSingleParameter(dbCommand, tempParamInfo.Name, tempParamInfo, databaseUnit);
                    }

                    sql = GetInListSql(sql, paramInfo.Name, count);
                }
                //参数为单个类型
                else
                {
                    SetUpSingleParameter(dbCommand, info.Key, info.Value, databaseUnit);
                }
            }
        }

        public static void SetUpSingleParameter(this IDbCommand dbCommand, string parameterName, ParamInfo paramInfo,
            DatabaseUnit databaseUnit)
        {
            var parameter = dbCommand.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Direction = paramInfo.ParameterDirection;

            var isSetValue = true;
            if (paramInfo.DbType != null)
            {
                parameter.DbType = paramInfo.DbType.Value;
            }
            else
            {
                var propertyType = paramInfo.ValueType?? paramInfo.Value.GetType();

                if (propertyType.IsNullable())
                {
                    propertyType = Nullable.GetUnderlyingType(propertyType);
                }

                if (propertyType == null)
                {
                    throw new NotSupportedException(parameter.ParameterName);
                }

                if (propertyType.IsEnum)
                {
                    propertyType = Enum.GetUnderlyingType(propertyType);
                }

                if (DatabaseUnit.TypeHandlers.ContainsKey(databaseUnit.Id) &&
                    DatabaseUnit.TypeHandlers[databaseUnit.Id].ContainsKey(propertyType))
                {
                    var typeHandlerCache = DatabaseUnit.TypeHandlers[databaseUnit.Id][propertyType];
                    typeHandlerCache.GetMethod("SetValue").Invoke(null, new object[] { parameter, paramInfo.Value });
                    isSetValue = false;
                }

                else if (databaseUnit.ParameterTypeMaps.ContainsKey(propertyType) && databaseUnit.ParameterTypeMaps[propertyType].HasValue)
                {
                    parameter.DbType = databaseUnit.ParameterTypeMaps[propertyType].Value;
                }
                else
                {
                    throw new NotSupportedException(parameter.ParameterName + ":" + propertyType.Name);
                }

            }

            if (isSetValue)
            {
                if (paramInfo.Value is DBNull || paramInfo.Value is null)
                {
                    parameter.Value = DBNull.Value;
                }
                else
                {
                    parameter.Value = paramInfo.Value;
                }
            }

            paramInfo.AssociatedActualParameters = parameter;

            dbCommand.Parameters.Add(parameter);
        }

        private static string GetInListSql(string sql, string parameterName, int count)
        {
            var pattern = ("([?@:]" + Regex.Escape(parameterName) + @")(?!\w)(\s+(?i)unknown(?-i))?");
            var result = Regex.Replace(sql, pattern, match =>
            {
                var variableName = match.Groups[1].Value;
                if (match.Groups[2].Success)
                {
                    var suffix = match.Groups[2].Value;

                    var sb = new StringBuilder().Append(variableName).Append(1).Append(suffix);
                    for (int i = 2; i <= count; i++)
                    {
                        sb.Append(',').Append(variableName).Append(i).Append(suffix);
                    }
                    return sb.ToString();
                }
                else
                {
                    var sb = new StringBuilder().Append('(').Append(variableName).Append(1);
                    for (int i = 2; i <= count; i++)
                    {
                        sb.Append(',').Append(variableName).Append(i);
                    }
                    return sb.Append(')').ToString();
                }
            }, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);

            return result;
        }

        //通过索引获取值
        private static readonly MethodInfo GetItem = typeof(IDataRecord).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(it =>
                it.Name == "get_Item" && it.GetParameters().Length == 1 &&
                it.GetParameters()[0].ParameterType == typeof(int));

        private static object lockObj = new object();

        public static Func<IDataReader, object> GetDeserializer(Type type, IDataReader dr, DatabaseUnit databaseUnit)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);

            if (IsPrimitiveStructOrStringOrEnum(type) || IsPrimitiveStructOrStringOrEnum(underlyingType))
            {
                return GetStructDeserializer(type, underlyingType ?? type, databaseUnit);
            }

            return GetTypeDeserializer(type, dr, databaseUnit);
        }

        /// <summary>
        /// 判断是否为原生值类型或者字符串类型或者枚举
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsPrimitiveStructOrStringOrEnum(Type type)
        {
            if (type == null)
            {
                return false;
            }
            return type.IsPrimitiveValueType() || type.IsString() || type.IsEnum;
        }

        /// <summary>
        /// 读取dataReader转化为type类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Func<IDataReader, object> GetTypeDeserializer(Type type, IDataReader dr, DatabaseUnit databaseUnit)
        {
            var dynamicMethod = new DynamicMethod("GetTypeDeserializer" + Guid.NewGuid().ToString("N"), typeof(object),
                new Type[] { typeof(IDataReader) });
            var il = dynamicMethod.GetILGenerator();
            var errorIndexLocal = il.DeclareLocal(typeof(int));
            var returnValueLocal = il.DeclareLocal(type);

            var queryMemberCacheInfos = GetQueryMemberCacheInfos(dr, type);
            //数据库返回的结果集里都找不到要查询的属性，直接返回
            if (queryMemberCacheInfos.Count == 0)
            {
                if (type.IsValueType)
                {
                    il.Emit(OpCodes.Ldloca, returnValueLocal);
                    il.Emit(OpCodes.Initobj, type);
                }
                else
                {
                    var ctor = type.GetConstructor(Type.EmptyTypes);
                    if (ctor == null)
                    {
                        throw new Exception($"Type:{type.FullName} must have a parameterless constructor");
                    }
                    il.Emit(OpCodes.Newobj, ctor);
                    il.Emit(OpCodes.Stloc, returnValueLocal);
                }
                il.Emit(OpCodes.Ldloc, returnValueLocal);
                il.Emit(OpCodes.Ret);
                var tempResult = (Func<IDataReader, object>)dynamicMethod.CreateDelegate(typeof(Func<IDataReader, object>));

                return tempResult;
                //throw new Exception("The column to query could not be found");
            }

            //try
            var tyrLabel = il.BeginExceptionBlock();
            //定义返回值
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Ldloca, returnValueLocal);
                il.Emit(OpCodes.Initobj, type);
                il.Emit(OpCodes.Ldloca, returnValueLocal);
            }
            else
            {
                var ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                {
                    throw new Exception($"Type:{type.FullName} must have a parameterless constructor");
                }
                il.Emit(OpCodes.Newobj, ctor);
                il.Emit(OpCodes.Stloc, returnValueLocal);
                il.Emit(OpCodes.Ldloc, returnValueLocal);
            }

            //堆栈用[]表示，则当前为type的实例，即[target]

            var backUpObject = il.DeclareLocal(typeof(object));
            var endLabel = il.DefineLabel();
            queryMemberCacheInfos = queryMemberCacheInfos.OrderBy(it => it.DataReaderIndex).ToList();
            // 
            for (var i = 0; i < queryMemberCacheInfos.Count; i++)
            {
                var queryMemberCacheInfo = queryMemberCacheInfos[i];
                var drFieldType = dr.GetFieldType(queryMemberCacheInfo.DataReaderIndex);
                var entityFieldType = queryMemberCacheInfo.PropertyInfo.PropertyType;
                var nullableEntityFieldType = Nullable.GetUnderlyingType(entityFieldType);
                //实际类型
                var realType = nullableEntityFieldType ?? entityFieldType;

                var dbNullLabel = il.DefineLabel();
                var finishLabel = il.DefineLabel();
                il.Emit(OpCodes.Dup);// [target,target]
                il.EmitInt32(i);// [target,target,i]
                il.SteadOfLocal(errorIndexLocal);//[target,target]
                //通过索引从dataReader里读取数据，此时读取回来的是object类型
                il.Emit(OpCodes.Ldarg_0); //[target, target,dataReader]
                il.EmitInt32(queryMemberCacheInfo.DataReaderIndex);//[target, target,dataReader,i]
                il.Emit(OpCodes.Callvirt, GetItem);// [target, target, getItemValue]
                //判断返回值是否为dbnull，如果是，则跳转到结束
                il.Emit(OpCodes.Dup);// [target, target, getItemValue,getItemValue]
                il.Emit(OpCodes.Isinst, typeof(DBNull));// [target, target, getItemValue, bool]
                il.Emit(OpCodes.Brtrue_S, dbNullLabel);// [target, target, getItemValue]
                //il.Emit(OpCodes.Call, typeof(DatabaseContext).GetMethod(nameof(DebugObj)));
                //对获取到的值进行备份,存到字段backUpObject里
                il.Emit(OpCodes.Dup);// [target, target, getItemValue,getItemValue]
                il.SteadOfLocal(backUpObject);// [target, target, getItemValue]

                if (DatabaseUnit.TypeHandlers[databaseUnit.Id].ContainsKey(realType))
                {
                    //这里是一个静态类，可以直接调用。
                    var typeHandlerCacheType = DatabaseUnit.TypeHandlers[databaseUnit.Id][realType];
                    il.Emit(OpCodes.Call, typeHandlerCacheType.GetMethod("Parse"));
                    //判断是否为可空类型
                    if (nullableEntityFieldType != null)
                    {
                        il.Emit(OpCodes.Newobj, entityFieldType.GetConstructor(new[] { realType }));
                    }
                }
                else
                {
                    il.ConvertTypeToTargetType(drFieldType, entityFieldType);// [target, target, realValue]
                }

                il.Emit(OpCodes.Call, queryMemberCacheInfo.PropertyInfo.GetSetMethod()); //[target]
                il.Emit(OpCodes.Br_S, finishLabel);


                // il.Emit(OpCodes.Call,typeof(object).GetMethod(nameof(object.ToString),new []{typeof(string)}));
                // il.Emit(OpCodes.Box,typeof(object));
                // il.Emit(OpCodes.Call,typeof(DatabaseContext).GetMethod(nameof(DebugObj)));
                // il.Emit(OpCodes.Call,typeof(Console).GetMethod(nameof(Console.WriteLine),new []{typeof(object)}));

                il.MarkLabel(dbNullLabel);
                il.Emit(OpCodes.Pop);// [target, target]
                il.Emit(OpCodes.Pop);// [target]
                il.Emit(OpCodes.Br_S, finishLabel);
                il.MarkLabel(finishLabel);
            }
            il.MarkLabel(endLabel);
            il.SteadOfLocal(returnValueLocal);

            il.BeginCatchBlock(typeof(Exception));
            //此时栈顶元素为[exception]
            il.Emit(OpCodes.Ldloc, backUpObject);
            il.Emit(OpCodes.Ldloc, errorIndexLocal);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, typeof(DatabaseContext).GetMethod(nameof(ThrowRepositoryException)));
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ldloc, returnValueLocal);
            il.Emit(OpCodes.Ret);

            var result = (Func<IDataReader, object>)dynamicMethod.CreateDelegate(typeof(Func<IDataReader, object>));

            return result;
        }
        /// <summary>
        /// 读取datareader转为struct类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="effectiveType"></param>
        /// <returns></returns>
        private static Func<IDataReader, object> GetStructDeserializer(Type type, Type effectiveType, DatabaseUnit databaseUnit)
        {
            if (DatabaseUnit.TypeHandlers[databaseUnit.Id].ContainsKey(effectiveType))
            {
                var typeHandlerCacheType = DatabaseUnit.TypeHandlers[databaseUnit.Id][effectiveType];
                return r =>
                {

                    var val = r.GetValue(0);
                    if (val is DBNull)
                    {
                        return null;
                    }

                    var result = typeHandlerCacheType.GetMethod("Parse").Invoke(null, new object[] { val });
                    return result;
                };
            }
            //最普通的值类型
            return r =>
            {

                var val = r.GetValue(0);
                return val is DBNull ? null : val;
            };
        }

        public static void DebugBreakPoint()
        {
            var st = new StackTrace();
        }
        public static void DebugObj(Exception ex)
        {

        }
        public static void ThrowRepositoryException(Exception ex, object value, int index, IDataReader reader)
        {
            Exception toThrow;
            try
            {
                string name = "", formattedValue = "";
                if (reader != null && index >= 0 && index < reader.FieldCount)
                {
                    name = reader.GetName(index);

                    try
                    {
                        if (value == null || value is DBNull)
                        {
                            formattedValue = "<null>";
                        }
                        else
                        {
                            formattedValue = Convert.ToString(value) + " - " + Type.GetTypeCode(value.GetType());
                        }
                    }
                    catch (Exception valEx)
                    {
                        formattedValue = valEx.Message;
                    }
                }
                toThrow = new DataException($"Error parsing column {index} ({name}={formattedValue})", ex);
            }
            catch
            { // throw the **original** exception, wrapped as DataException
                toThrow = new DataException(ex.Message, ex);
            }
            throw toThrow;
        }


        /// <summary>
        /// 获取要查询的属性列表
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static List<MemberInfoCache> GetQueryMemberCacheInfos(IDataReader dr, Type type)
        {
            var memberCacheInfos = type.GetMemberInfoCachesForSetting();
            var tableColNames = Enumerable.Range(0, dr.FieldCount).Select(it => new { name = dr.GetName(it), index = it }).ToList();
            var result = new List<MemberInfoCache>();
            foreach (var info in memberCacheInfos)
            {
                var tableColName = tableColNames.FirstOrDefault(it =>
                    string.Equals(it.name, info.Name, StringComparison.InvariantCultureIgnoreCase));
                if (tableColName == null)
                {
                    continue;
                }

                info.DataReaderIndex = tableColName.index;
                result.Add(info);
            }

            return result;
        }

        /// <summary>
        /// 获取数据库类型
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static DatabaseType GetDatabaseType(this IDbConnection dbConnection)
        {
            var dbConnectionType = dbConnection.GetType();
            var dbName = dbConnectionType.FullName;
            if (dbName.ToLower().IndexOf("sqlite") > -1)
            {
                return DatabaseType.Sqlite;
            }
            else if (dbName.ToLower().IndexOf("mysql") > -1)
            {
                return DatabaseType.Mysql;
            }
            else if (dbName.ToLower().IndexOf("sqlconnection") > -1 && dbName.ToLower().IndexOf("microsoft") > -1
                      || (dbName.ToLower().IndexOf("sqlconnection") > -1 && dbName.ToLower().IndexOf("system") > -1))
            {
                return DatabaseType.SqlServer;
            }
            else if (dbName.ToLower().IndexOf("oracle") > -1)
            {
                return DatabaseType.Oracle;
            }
            else
            {
                throw new NotSupportedException(nameof(dbName));
            }
        }

        /// <summary>
        /// 动态生成TypeHandlerCache类
        /// </summary>
        /// <returns></returns>
        public static Type GenerateTypeHandlerCacheClass(Type defineType)
        {
            var name = "ITest";
            string assemblyName = name + "ProxyAssembly";
            string moduleName = name + "ProxyModule";
            string typeName = name + "Proxy";

            AssemblyName assyName = new AssemblyName(assemblyName);
            AssemblyBuilder assyBuilder = AssemblyBuilder.DefineDynamicAssembly(assyName, AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = assyBuilder.DefineDynamicModule(moduleName);
            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Public | TypeAttributes.Abstract |
                                              TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit |
                                              TypeAttributes.Class;

            //父类型
            Type parentType;
            //要实现的接口
            Type[] interfaceTypes = Type.EmptyTypes;
            parentType = null;

            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType("GenerateTypeHandlerCacheClass", newTypeAttribute, parentType, interfaceTypes);

            var typeParams = typeBuilder.DefineGenericParameters("T");

            GenericTypeParameterBuilder first = typeParams[0];
            //first.SetGenericParameterAttributes(
            //    GenericParameterAttributes.ReferenceTypeConstraint);
            var typeHandlerType = typeof(ITypeHandler);

            var staticTypeHandlerField = typeBuilder.DefineField("handler", typeHandlerType,
                FieldAttributes.Public | FieldAttributes.Static);

            //SetValue方法
            var staticSetValueMethod = typeBuilder.DefineMethod("SetValue", MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard, null, new Type[] { typeof(IDbDataParameter), typeof(object) });
            var ilGenerator = staticSetValueMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldsfld, staticTypeHandlerField);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Callvirt, typeHandlerType.GetMethod("SetValue"));
            ilGenerator.Emit(OpCodes.Ret);

            //SetHandler方法
            var staticSetMethod = typeBuilder.DefineMethod("SetHandler", MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard, null, new Type[] { typeHandlerType });
            var ilG2 = staticSetMethod.GetILGenerator();
            ilG2.Emit(OpCodes.Ldarg_0);
            ilG2.Emit(OpCodes.Stsfld, staticTypeHandlerField);
            ilG2.Emit(OpCodes.Ret);

            //Parse方法
            var staticParseMethod = typeBuilder.DefineMethod("Parse", MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard, first, new Type[] { typeof(object) });

            var ilParseGenerator = staticParseMethod.GetILGenerator();
            ilParseGenerator.Emit(OpCodes.Ldsfld, staticTypeHandlerField);
            ilParseGenerator.Emit(OpCodes.Ldtoken, first);
            ilParseGenerator.Emit(OpCodes.Call, SbUtil.GetTypeFromHandleMethod);
            ilParseGenerator.Emit(OpCodes.Ldarg_0);
            ilParseGenerator.Emit(OpCodes.Callvirt, typeHandlerType.GetMethod("Parse"));
            ilParseGenerator.Emit(OpCodes.Unbox_Any, first);
            ilParseGenerator.Emit(OpCodes.Ret);

            var resultType = typeBuilder.CreateTypeInfo().AsType();
            resultType = resultType.MakeGenericType(defineType);
            return resultType;
        }
    }
}
