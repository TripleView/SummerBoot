using SummerBoot.Repository;
using System;
using System.Collections.Generic;
using System.Data;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        public static Dictionary<Type, DbType> CsharpTypeToDbTypeMap = new Dictionary<Type, DbType>()
        {
            {typeof(object), DbType.Object},
            {typeof(byte), DbType.Byte},
            {typeof(sbyte), DbType.SByte},
            {typeof(short), DbType.Int16},
            {typeof(ushort), DbType.UInt16},
            {typeof(int), DbType.Int32},
            {typeof(uint), DbType.UInt32},
            {typeof(long), DbType.Int64},
            {typeof(ulong), DbType.UInt64},
            {typeof(float), DbType.Single},
            {typeof(double), DbType.Double},
            {typeof(decimal), DbType.Decimal},
            {typeof(bool), DbType.Boolean},
            {typeof(string), DbType.String},
            {typeof(char), DbType.StringFixedLength},
            {typeof(Guid), DbType.Guid},
            {typeof(DateTime), DbType.DateTime},
            {typeof(DateTimeOffset), DbType.DateTimeOffset},
            {typeof(TimeSpan), DbType.Time},
            {typeof(byte[]), DbType.Binary},
            {typeof(byte?), DbType.Byte},
            {typeof(sbyte?), DbType.SByte},
            {typeof(short?), DbType.Int16},
            {typeof(ushort?), DbType.UInt16},
            {typeof(int?), DbType.Int32},
            {typeof(uint?), DbType.UInt32},
            {typeof(long?), DbType.Int64},
            {typeof(ulong?), DbType.UInt64},
            {typeof(float?), DbType.Single},
            {typeof(double?), DbType.Double},
            {typeof(decimal?), DbType.Decimal},
            {typeof(bool?), DbType.Boolean},
            {typeof(char?), DbType.StringFixedLength},
            {typeof(Guid?), DbType.Guid},
            {typeof(DateTime?), DbType.DateTime},
            {typeof(DateTimeOffset?), DbType.DateTimeOffset},
            {typeof(TimeSpan?), DbType.Time}
        };

        /// <summary>
        /// .NET数据类型与DbType对应关系
        /// </summary>
        public static DbType CSharpTypeToDbType(this Type type, DatabaseType databaseType)
        {
            var tmpType = Nullable.GetUnderlyingType(type);
            if (tmpType != null)
            {
                type = tmpType;
            }

            DbType result;
            if (CsharpTypeToDbTypeMap.ContainsKey(type))
            {
                result = CsharpTypeToDbTypeMap[type];
            }else if (type.IsEnum)
            {
                result = DbType.Byte;
            }
            else
            {
                throw new NotSupportedException($"Failed to convert {type.FullName} to dbType");
            }

            if (databaseType == DatabaseType.Oracle)
            {
                if (result == DbType.Guid)
                {
                    result = DbType.Binary;
                }
                if (result == DbType.Boolean)
                {
                    result = DbType.Byte;
                }
            }

            return result;
        }
    }


}