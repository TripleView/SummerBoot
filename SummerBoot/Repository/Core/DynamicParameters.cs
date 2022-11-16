using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Xml.Linq;
using SummerBoot.Core;
using YamlDotNet.Core.Tokens;

namespace SummerBoot.Repository.Core
{
    public class DynamicParameters
    {
        private readonly Dictionary<string, ParamInfo> paramInfos = new Dictionary<string, ParamInfo>();

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

        public void Add(string name, object value = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            name = CleanParameterName(name);
            paramInfos[name] = new ParamInfo
            {
                Name = name,
                Value = value,
                ParameterDirection = direction ?? ParameterDirection.Input,
                DbType = dbType,
                Size = size,
                Precision = precision,
                Scale = scale
            };
        }

        public void AddEntity<T>(T entity)
        {
            var type= entity.GetType();

            if (type.IsValueType && type.IsPrimitive||(type.IsString()))
            {
                throw new NotSupportedException("entity must be object");
            }
            var memberInfos=type.GetMemberInfoCachesForGetting();
            foreach (var memberInfoCache in memberInfos)
            {
                var name = memberInfoCache.PropertyName;
                var value = entity.GetPropertyValueByEmit(memberInfoCache.PropertyName);
                paramInfos[name] = new ParamInfo
                {
                    Name = name,
                    Value = value,
                    ParameterDirection = ParameterDirection.Input
                };
            }
        }
    }
}
