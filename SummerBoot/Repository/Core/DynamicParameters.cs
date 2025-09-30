using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using SummerBoot.Core;
using YamlDotNet.Core.Tokens;

namespace SummerBoot.Repository.Core
{
    public class DynamicParameters
    {
        public DynamicParameters() { }

        public DynamicParameters(object entity)
        {
            if (entity is DynamicParameters dp)
            {
                foreach (var dpGetParamInfo in dp.GetParamInfos)
                {
                    this.AddParamInfo(dpGetParamInfo.Key, dpGetParamInfo.Value);
                }
            }
            else if (entity is IDictionary<string, object> dic)
            {
                foreach (var pair in dic)
                {
                    this.Add(pair.Key, pair.Value);
                }
            }
            else
            {
                this.AddEntity(entity);
            }

        }
        private readonly Dictionary<string, ParamInfo> paramInfos = new Dictionary<string, ParamInfo>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ParamInfo> GetParamInfos => paramInfos;
        /// <summary>
        /// 清除参数名称里的参数标识符
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string CleanParameterName(string parameterName)
        {
            if (parameterName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(parameterName);
            }

            if (parameterName.Length > 0 && RepositoryOption.ParameterIdentifiers.Contains(parameterName[0]))
            {
                return parameterName.Substring(1);
            }

            return parameterName;
        }

        public T Get<T>(string name)
        {
            var paramInfo = paramInfos[CleanParameterName(name)];

            object val = paramInfo.AssociatedActualParameters != null ? paramInfo.AssociatedActualParameters.Value : paramInfo.Value;
            if (val == DBNull.Value)
            {
                if (default(T) != null)
                {
                    throw new ApplicationException("Attempting to cast a DBNull to a non nullable type! Note that out/return parameters will not have updated values until the data stream completes (after the 'foreach' for Query(..., buffered: false), or after the GridReader has been disposed for QueryMultiple)");
                }
                return default;
            }
            return (T)val;
        }

        public void Add(string name, object value = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null, Type valueType = null)
        {
            var paramInfo = new ParamInfo
            {
                Name = name,
                Value = value,
                ParameterDirection = direction ?? ParameterDirection.Input,
                DbType = dbType,
                Size = size,
                Precision = precision,
                Scale = scale,
                ValueType = valueType
            };
            this.AddParamInfo(name, paramInfo);
        }

        private void AddParamInfo(string name, ParamInfo paramInfo)
        {
            name = CleanParameterName(name);
            paramInfo.Name = name;
            paramInfos[name] = paramInfo;
        }

        public void AddEntity<T>(T entity)
        {
            CheckHelper.NotNull(entity, typeof(T).Name);
            var type = entity.GetType();

            if (type.IsValueType && type.IsPrimitive || (type.IsString()))
            {
                throw new NotSupportedException("entity must be object");
            }
            var memberInfos = type.GetMemberInfoCachesForGetting();
            foreach (var memberInfoCache in memberInfos)
            {
                var name = memberInfoCache.Name;
                var value = entity.GetPropertyValueByEmit(memberInfoCache.PropertyName);
                var paramInfo = new ParamInfo
                {
                    Name = name,
                    Value = value,
                    ParameterDirection = ParameterDirection.Input,
                    ValueType = memberInfoCache.PropertyInfo.PropertyType
                };
                this.AddParamInfo(name, paramInfo);

            }
        }
    }
}
