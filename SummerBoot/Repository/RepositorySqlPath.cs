using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public class RepositorySqlPath : IRepositorySqlPath
    {
        private Dictionary<string, string> dic;
        public void AddPath(string modelName, string sql)
        {
            if (dic.ContainsKey(modelName)) throw new ArgumentException(modelName + "exist ");
            dic.Add(modelName,sql);
        }

        public string GetPath(string modelName)
        {
            return "";
        }
    }
}
